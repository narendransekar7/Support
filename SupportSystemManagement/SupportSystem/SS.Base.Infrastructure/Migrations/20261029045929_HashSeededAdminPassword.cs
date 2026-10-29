using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS.Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HashSeededAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "UserId",
                keyValue: new Guid("6437f734-63ef-421e-9a48-4fd33978671f"),
                column: "Password",
                value: "AQAAAAIAAYagAAAAEMJPNHTzUk1r2SPrM/l6v6WFpr53bKysIvUodtPpm/HBPiV/Mbwl8Tkbw1AVnVGetw==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "UserId",
                keyValue: new Guid("6437f734-63ef-421e-9a48-4fd33978671f"),
                column: "Password",
                value: "Admin@123");
        }
    }
}
