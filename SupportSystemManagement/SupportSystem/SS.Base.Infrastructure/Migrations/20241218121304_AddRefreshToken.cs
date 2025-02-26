using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS.Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserProfiles",
                keyColumn: "UserId",
                keyValue: new Guid("45c0aa78-7435-485d-838e-e7a7f66eb7fb"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("45c0aa78-7435-485d-838e-e7a7f66eb7fb"));

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Token = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Token);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "DisplayName", "FirstName", "LastName", "PrimaryEmail", "Role" },
                values: new object[] { new Guid("c7826bad-4b94-4a03-b068-d92b17234c65"), "Admin User", "Admin", "User", "admin@gmail.com", 0 });

            migrationBuilder.InsertData(
                table: "UserProfiles",
                columns: new[] { "UserId", "Country", "Gender", "Password", "PrimaryNumber" },
                values: new object[] { new Guid("c7826bad-4b94-4a03-b068-d92b17234c65"), "US", "Male", "Admin@123", "1234567890" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DeleteData(
                table: "UserProfiles",
                keyColumn: "UserId",
                keyValue: new Guid("c7826bad-4b94-4a03-b068-d92b17234c65"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("c7826bad-4b94-4a03-b068-d92b17234c65"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "DisplayName", "FirstName", "LastName", "PrimaryEmail", "Role" },
                values: new object[] { new Guid("45c0aa78-7435-485d-838e-e7a7f66eb7fb"), "Admin User", "Admin", "User", "admin@gmail.com", 0 });

            migrationBuilder.InsertData(
                table: "UserProfiles",
                columns: new[] { "UserId", "Country", "Gender", "Password", "PrimaryNumber" },
                values: new object[] { new Guid("45c0aa78-7435-485d-838e-e7a7f66eb7fb"), "US", "Male", "Admin@123", "1234567890" });
        }
    }
}
