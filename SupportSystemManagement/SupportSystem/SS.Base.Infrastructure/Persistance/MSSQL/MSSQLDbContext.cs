using Microsoft.EntityFrameworkCore;
using SS.Base.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Infrastructure.Persistance.MSSQL
{
    public class MSSQLDbContext : DbContext
    {
        public MSSQLDbContext(DbContextOptions<MSSQLDbContext> options): base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        public DbSet<UserProfile> UserProfiles { get; set; }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketLog> TicketLogs { get; set; }
        public DbSet<TicketUpdate> TicketUpdates { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RoundRobinCursor> RoundRobinCursors { get; set; }
        public DbSet<TicketCreationSagaState> TicketCreationSagaStates { get; set; }
        
        // Seeding method
        public static void Seed(ModelBuilder modelBuilder)
        {
            // Fixed (not random) so the EF model is deterministic across builds —
            // a random value here made every `dotnet ef migrations add` scaffold a
            // spurious delete+reinsert of the seeded admin user.
            Guid adminId = new Guid("6437f734-63ef-421e-9a48-4fd33978671f");
            // Add initial data for User table
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = adminId,
                    FirstName = "Admin",
                    LastName = "User",
                    DisplayName = "Admin User",
                    PrimaryEmail = "admin@gmail.com",
                    Role = Role.Admin // Assuming 'Role' is an enum or predefined set
                }
            );

            // Add initial data for UserProfile table
            modelBuilder.Entity<UserProfile>().HasData(
                new UserProfile
                {
                    UserId = adminId,
                    Password = "Admin@123", // Make sure this is a hashed password in production
                    Country = "US",
                    Gender = "Male",
                    PrimaryNumber = "1234567890"
                }
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            MSSQLDbContext.Seed(modelBuilder);
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PrimaryEmail).IsRequired().HasMaxLength(50);

                // UserProfile Relationship
                entity.HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId);

                // Ticket Relationship — a ticket may be unassigned until the
                // saga's Assign Engineer step runs, so this is optional.
                entity.HasMany(u => u.Tickets)
                 .WithOne(t => t.User)
                 .HasForeignKey(t => t.AssignedTo)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Ticket entity
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => t.TicketId);
                entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(t => t.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasMany(tu => tu.TicketUpdates)
                .WithOne(t => t.Ticket)
                .HasForeignKey(tu => tu.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TicketUpdate entity
            modelBuilder.Entity<TicketUpdate>(entity =>
            {
                entity.HasKey(e => e.UpdateId);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.UpdatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.NotificationId);
                entity.Property(n => n.Message).IsRequired().HasMaxLength(1000);
                entity.Property(n => n.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure RoundRobinCursor entity (single row, Id always explicitly 1 — not an auto-increment key)
            modelBuilder.Entity<RoundRobinCursor>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedNever();
            });

            // Configure TicketCreationSagaState entity (MassTransit saga persistence)
            modelBuilder.Entity<TicketCreationSagaState>(entity =>
            {
                entity.HasKey(s => s.CorrelationId);
                entity.Property(s => s.CurrentState).IsRequired().HasMaxLength(64);
                entity.Property(s => s.Title).HasMaxLength(200);
                entity.Property(s => s.Priority).HasMaxLength(20);
                entity.Property(s => s.CreatedByEmail).HasMaxLength(100);
                entity.Property(s => s.CreatedByName).HasMaxLength(100);
                entity.Property(s => s.FailureReason).HasMaxLength(500);
                entity.Property(s => s.RowVersion).IsRowVersion();
            });

        }

        //previous
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Configure User entity
        //    modelBuilder.Entity<User>(entity =>
        //    {
        //        entity.HasKey(u => u.UserId);
        //        entity.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
        //        entity.Property(u => u.LastName).IsRequired().HasMaxLength(50);
        //        entity.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
        //        entity.Property(u => u.PrimaryEmail).IsRequired().HasMaxLength(50);
        //    });

        //    //Configure Ticket entity
        //    modelBuilder.Entity<Ticket>()
        //        .HasOne(t => t.User)
        //        .WithMany(u => u.Tickets)
        //        .HasForeignKey(t => t.AssignedTo)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    // Configure TicketUpdate entity
        //    modelBuilder.Entity<TicketUpdate>(entity =>
        //    {
        //        entity.HasKey(e => e.UpdateId);
        //        entity.Property(e => e.Content).IsRequired();
        //        entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

        //        entity.HasOne(e => e.Ticket)
        //              .WithMany(t => t.TicketUpdates)
        //              .HasForeignKey(e => e.TicketId)
        //              .OnDelete(DeleteBehavior.Cascade);

        //        entity.HasOne(e => e.UpdatedByUser)
        //              .WithMany()
        //              .HasForeignKey(e => e.UpdatedBy)
        //              .OnDelete(DeleteBehavior.Restrict);
        //    });

        //}




    }
}
