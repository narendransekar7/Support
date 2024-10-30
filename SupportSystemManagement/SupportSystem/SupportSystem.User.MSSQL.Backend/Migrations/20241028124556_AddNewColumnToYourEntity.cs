using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportSystem.User.MSSQL.Backend.Migrations
{
    public partial class AddNewColumnToYourEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "SS_User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "SS_User");
        }
    }
}
