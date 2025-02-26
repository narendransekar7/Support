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
        
        // Seeding method
        public static void Seed(ModelBuilder modelBuilder)
        {
            Guid adminId = Guid.NewGuid();
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

                // Ticket Relationship
                entity.HasMany(u => u.Tickets)
                 .WithOne(t => t.User)
                 .HasForeignKey(t => t.AssignedTo)
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
