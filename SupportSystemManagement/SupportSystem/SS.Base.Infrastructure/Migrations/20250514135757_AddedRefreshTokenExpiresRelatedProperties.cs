using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS.Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedRefreshTokenExpiresRelatedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserProfiles",
                keyColumn: "UserId",
                keyValue: new Guid("c7826bad-4b94-4a03-b068-d92b17234c65"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("c7826bad-4b94-4a03-b068-d92b17234c65"));

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "RefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "DisplayName", "FirstName", "LastName", "PrimaryEmail", "Role" },
                values: new object[] { new Guid("a62ed1c5-473f-4979-b7cb-4f97bc42fb38"), "Admin User", "Admin", "User", "admin@gmail.com", 0 });

            migrationBuilder.InsertData(
                table: "UserProfiles",
                columns: new[] { "UserId", "Country", "Gender", "Password", "PrimaryNumber" },
                values: new object[] { new Guid("a62ed1c5-473f-4979-b7cb-4f97bc42fb38"), "US", "Male", "Admin@123", "1234567890" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserProfiles",
                keyColumn: "UserId",
                keyValue: new Guid("a62ed1c5-473f-4979-b7cb-4f97bc42fb38"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("a62ed1c5-473f-4979-b7cb-4f97bc42fb38"));

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "RefreshTokens");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "DisplayName", "FirstName", "LastName", "PrimaryEmail", "Role" },
                values: new object[] { new Guid("c7826bad-4b94-4a03-b068-d92b17234c65"), "Admin User", "Admin", "User", "admin@gmail.com", 0 });

            migrationBuilder.InsertData(
                table: "UserProfiles",
                columns: new[] { "UserId", "Country", "Gender", "Password", "PrimaryNumber" },
                values: new object[] { new Guid("c7826bad-4b94-4a03-b068-d92b17234c65"), "US", "Male", "Admin@123", "1234567890" });
        }
    }
}
