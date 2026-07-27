using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS.Base.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRoundRobinCursorIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server can't ALTER a column's IDENTITY property in place, and
            // EF's structured Drop/CreateTable-in-one-migration path trips over
            // reusing the same table name — raw SQL sidesteps both issues (the
            // table is new/empty at this point, so a drop+recreate is safe).
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS [RoundRobinCursors];
                CREATE TABLE [RoundRobinCursors] (
                    [Id] int NOT NULL,
                    [LastAssignedUserId] uniqueidentifier NULL,
                    CONSTRAINT [PK_RoundRobinCursors] PRIMARY KEY ([Id])
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS [RoundRobinCursors];
                CREATE TABLE [RoundRobinCursors] (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [LastAssignedUserId] uniqueidentifier NULL,
                    CONSTRAINT [PK_RoundRobinCursors] PRIMARY KEY ([Id])
                );
            ");
        }
    }
}
