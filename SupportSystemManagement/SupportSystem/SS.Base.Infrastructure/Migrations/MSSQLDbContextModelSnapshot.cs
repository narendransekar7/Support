﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SS.Base.Infrastructure.Persistance.MSSQL;

#nullable disable

namespace SS.Base.Infrastructure.Migrations
{
    [DbContext(typeof(MSSQLDbContext))]
    partial class MSSQLDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SS.Base.Domain.Entities.RefreshToken", b =>
                {
                    b.Property<Guid>("Token")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsRevoked")
                        .HasColumnType("bit");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Token");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("SS.Base.Domain.Entities.Ticket", b =>
                {
                    b.Property<Guid>("TicketId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AssignedTo")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Priority")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ResolutionDueDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ResponseDueDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("Visibility")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TicketId");

                    b.HasIndex("AssignedTo");

                    b.ToTable("Tickets");
                });

            modelBuilder.Entity("SS.Base.Domain.Entities.TicketLog", b =>
                {
                    b.Property<Guid>("LogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NewValue")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OldValue")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("PerformedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("PerformedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TicketId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("LogId");

                    b.ToTable("TicketLogs");
                });

            modelBuilder.Entity("SS.Base.Domain.Entities.TicketUpdate", b =>
                {
                    b.Property<Guid>("UpdateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<Guid>("TicketId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<Guid>("UpdatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UpdateId");

                    b.HasIndex("TicketId");

                    b.HasIndex("UpdatedBy");

                    b.ToTable("TicketUpdates");
                });

            modelBuilder.Entity("SS.Base.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PrimaryEmail")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.HasKey("UserId");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            UserId = new Guid("c7826bad-4b94-4a03-b068-d92b17234c65"),
                            DisplayName = "Admin User",
                            FirstName = "Admin",
                            LastName = "User",
                            PrimaryEmail = "admin@gmail.com",
                            Role = 0
                        });
                });

            modelBuilder.Entity("SS.Base.Domain.Entities.UserProfile", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Country")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PrimaryNumber")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.ToTable("UserProfiles");

                    b.HasData(
                        new
                        {
                            UserId = new Guid("c7826bad-4b94-4a03-b068-d92b17234c65"),
                            Country = "US",
                            Gender = "Male",
                            Password = "Admin@123",
                            PrimaryNumber = "1234567890"
                        });
                });

            modelBuilder.Entity("SS.Base.Domain.Entities.Ticket", b =>
                {
                    b.HasOne("SS.Base.Domain.Entities.User", "User")
                        .WithMany("Tickets")
                        .HasForeignKey("AssignedTo")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("SS.Base.Domain.Entities.TicketUpdate", b =>
                {
                    b.HasOne("SS.Base.Domain.Entities.Ticket", "Ticket")
                        .WithMany("TicketUpdates")
                        .HasForeignKey("TicketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SS.Base.Domain.Entities.User", "UpdatedByUser")
                        .WithMany()
                        .HasForeignKey("UpdatedBy")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Ticket");

                    b.Navigation("UpdatedByUser");
                });

            modelBuilder.Entity("SS.Base.Domain.Entities.UserProfile", b =>
                {
                    b.HasOne("SS.Base.Domain.Entities.User", "User")
                        .WithOne("Profile")
                        .HasForeignKey("SS.Base.Domain.Entities.UserProfile", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("SS.Base.Domain.Entities.Ticket", b =>
                {
                    b.Navigation("TicketUpdates");
                });

            modelBuilder.Entity("SS.Base.Domain.Entities.User", b =>
                {
                    b.Navigation("Profile")
                        .IsRequired();

                    b.Navigation("Tickets");
                });
#pragma warning restore 612, 618
        }
    }
}
