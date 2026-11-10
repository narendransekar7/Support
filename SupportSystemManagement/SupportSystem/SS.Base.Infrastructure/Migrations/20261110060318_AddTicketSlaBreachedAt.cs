using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS.Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketSlaBreachedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SlaBreachedAt",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ResolutionDueDate_Unreported",
                table: "Tickets",
                column: "ResolutionDueDate",
                filter: "[SlaBreachedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_ResolutionDueDate_Unreported",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SlaBreachedAt",
                table: "Tickets");
        }
    }
}
