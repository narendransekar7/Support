using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS.Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "DisplayName", "FirstName", "LastName", "PrimaryEmail", "Role" },
                values: new object[] { new Guid("45c0aa78-7435-485d-838e-e7a7f66eb7fb"), "Admin User", "Admin", "User", "admin@gmail.com", 0 });

            migrationBuilder.InsertData(
                table: "UserProfiles",
                columns: new[] { "UserId", "Country", "Gender", "Password", "PrimaryNumber" },
                values: new object[] { new Guid("45c0aa78-7435-485d-838e-e7a7f66eb7fb"), "US", "Male", "Admin@123", "1234567890" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserProfiles",
                keyColumn: "UserId",
                keyValue: new Guid("45c0aa78-7435-485d-838e-e7a7f66eb7fb"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("45c0aa78-7435-485d-838e-e7a7f66eb7fb"));
        }
    }
}
