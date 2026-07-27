using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS.Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketSagaAndSplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserProfiles",
                keyColumn: "UserId",
                keyValue: new Guid("e8157b38-5303-4998-936b-ba0bfdbe055a"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("e8157b38-5303-4998-936b-ba0bfdbe055a"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "ResponseDueDate",
                table: "Tickets",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ResolutionDueDate",
                table: "Tickets",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedTo",
                table: "Tickets",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "RoundRobinCursors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastAssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundRobinCursors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketCreationSagaStates",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedByEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedByName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCreationSagaStates", x => x.CorrelationId);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "DisplayName", "FirstName", "LastName", "PrimaryEmail", "Role" },
                values: new object[] { new Guid("ba3a3165-0248-48db-b211-343ab519fb11"), "Admin User", "Admin", "User", "admin@gmail.com", 0 });

            migrationBuilder.InsertData(
                table: "UserProfiles",
                columns: new[] { "UserId", "Country", "Gender", "Password", "PrimaryNumber" },
                values: new object[] { new Guid("ba3a3165-0248-48db-b211-343ab519fb11"), "US", "Male", "Admin@123", "1234567890" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "RoundRobinCursors");

            migrationBuilder.DropTable(
                name: "TicketCreationSagaStates");

            migrationBuilder.DeleteData(
                table: "UserProfiles",
                keyColumn: "UserId",
                keyValue: new Guid("ba3a3165-0248-48db-b211-343ab519fb11"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("ba3a3165-0248-48db-b211-343ab519fb11"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "ResponseDueDate",
                table: "Tickets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ResolutionDueDate",
                table: "Tickets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedTo",
                table: "Tickets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "DisplayName", "FirstName", "LastName", "PrimaryEmail", "Role" },
                values: new object[] { new Guid("e8157b38-5303-4998-936b-ba0bfdbe055a"), "Admin User", "Admin", "User", "admin@gmail.com", 0 });

            migrationBuilder.InsertData(
                table: "UserProfiles",
                columns: new[] { "UserId", "Country", "Gender", "Password", "PrimaryNumber" },
                values: new object[] { new Guid("e8157b38-5303-4998-936b-ba0bfdbe055a"), "US", "Male", "Admin@123", "1234567890" });
        }
    }
}
