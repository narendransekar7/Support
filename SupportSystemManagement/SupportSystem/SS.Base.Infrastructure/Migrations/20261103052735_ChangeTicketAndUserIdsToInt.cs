using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS.Base.Infrastructure.Migrations
{
    /// <summary>
    /// Users.UserId and Tickets.TicketId become int identity columns instead of Guids, and every column
    /// that holds a user or ticket id becomes int too. Hand-written (EF's scaffold would drop the data):
    /// existing rows get new ids through two map tables - the seeded admin becomes user 1, other users
    /// follow by email, tickets are numbered in creation order - and every reference is rewritten.
    /// Rows pointing at a user or ticket that no longer exists can't be mapped: notifications, ticket logs,
    /// sagas and refresh tokens for them are deleted, and a ticket whose creator is gone goes to the admin.
    /// </summary>
    public partial class ChangeTicketAndUserIdsToInt : Migration
    {
        private const string AdminId = "6437f734-63ef-421e-9a48-4fd33978671f";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Old Guid -> new int id, for users and tickets.
            migrationBuilder.Sql("CREATE TABLE [__UserIdMap] ([OldId] uniqueidentifier NOT NULL PRIMARY KEY, [NewId] int NOT NULL UNIQUE);");
            migrationBuilder.Sql($"INSERT INTO [__UserIdMap] ([OldId], [NewId]) SELECT [UserId], 1 FROM [Users] WHERE [UserId] = '{AdminId}';");
            migrationBuilder.Sql($@"INSERT INTO [__UserIdMap] ([OldId], [NewId])
SELECT [UserId], 1 + ROW_NUMBER() OVER (ORDER BY [PrimaryEmail], [UserId]) FROM [Users] WHERE [UserId] <> '{AdminId}';");
            migrationBuilder.Sql("CREATE TABLE [__TicketIdMap] ([OldId] uniqueidentifier NOT NULL PRIMARY KEY, [NewId] int NOT NULL UNIQUE);");
            migrationBuilder.Sql(@"INSERT INTO [__TicketIdMap] ([OldId], [NewId])
SELECT [TicketId], ROW_NUMBER() OVER (ORDER BY [CreatedAt], [TicketId]) FROM [Tickets];");

            // 2. Everything that pins the old id columns.
            migrationBuilder.DropForeignKey(name: "FK_TicketUpdates_Tickets_TicketId", table: "TicketUpdates");
            migrationBuilder.DropForeignKey(name: "FK_TicketUpdates_Users_UpdatedBy", table: "TicketUpdates");
            migrationBuilder.DropForeignKey(name: "FK_Tickets_Users_AssignedTo", table: "Tickets");
            migrationBuilder.DropForeignKey(name: "FK_UserProfiles_Users_UserId", table: "UserProfiles");
            migrationBuilder.DropIndex(name: "IX_Tickets_AssignedTo", table: "Tickets");
            migrationBuilder.DropIndex(name: "IX_TicketUpdates_TicketId", table: "TicketUpdates");
            migrationBuilder.DropIndex(name: "IX_TicketUpdates_UpdatedBy", table: "TicketUpdates");
            migrationBuilder.DropIndex(name: "IX_Notifications_UserId_IsRead", table: "Notifications");
            migrationBuilder.DropIndex(name: "IX_AuditLogs_TicketId_OccurredAtUtc", table: "AuditLogs");
            migrationBuilder.DropIndex(name: "IX_AuditLogs_UserId_OccurredAtUtc", table: "AuditLogs");
            migrationBuilder.DropPrimaryKey(name: "PK_UserProfiles", table: "UserProfiles");
            migrationBuilder.DropPrimaryKey(name: "PK_Users", table: "Users");
            migrationBuilder.DropPrimaryKey(name: "PK_Tickets", table: "Tickets");

            // 3. Rows whose user/ticket is gone (non-null columns that can't be mapped).
            migrationBuilder.Sql("DELETE FROM [Notifications] WHERE [UserId] NOT IN (SELECT [OldId] FROM [__UserIdMap]);");
            migrationBuilder.Sql(@"DELETE FROM [TicketLogs] WHERE [TicketId] NOT IN (SELECT [OldId] FROM [__TicketIdMap])
    OR [PerformedBy] NOT IN (SELECT [OldId] FROM [__UserIdMap]);");
            migrationBuilder.Sql("DELETE FROM [TicketCreationSagaStates] WHERE [TicketId] NOT IN (SELECT [OldId] FROM [__TicketIdMap]);");
            migrationBuilder.Sql("DELETE FROM [RefreshTokens] WHERE TRY_CONVERT(uniqueidentifier, [UserId]) IS NULL OR TRY_CONVERT(uniqueidentifier, [UserId]) NOT IN (SELECT [OldId] FROM [__UserIdMap]);");
            migrationBuilder.Sql($"UPDATE [Tickets] SET [CreatedBy] = '{AdminId}' WHERE [CreatedBy] NOT IN (SELECT [OldId] FROM [__UserIdMap]);");

            // 4. Users and Tickets are rebuilt: an existing column can't be made an identity column.
            // Renames and drops are plain SQL: EF's RenameTable followed by CreateTable of the same name fails.
            migrationBuilder.Sql("EXEC sp_rename N'[Users]', N'Users_Old';");
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrimaryEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });
            // One batch: IDENTITY_INSERT is per session, but keep ON/INSERT/OFF together regardless.
            migrationBuilder.Sql(@"SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [FirstName], [LastName], [DisplayName], [PrimaryEmail], [Role])
SELECT m.[NewId], u.[FirstName], u.[LastName], u.[DisplayName], u.[PrimaryEmail], u.[Role]
FROM [Users_Old] u JOIN [__UserIdMap] m ON m.[OldId] = u.[UserId];
SET IDENTITY_INSERT [Users] OFF;");
            migrationBuilder.Sql("DROP TABLE [Users_Old];");

            migrationBuilder.Sql("EXEC sp_rename N'[Tickets]', N'Tickets_Old';");
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ResolutionDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponseDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedTo = table.Column<int>(type: "int", nullable: true),
                    Visibility = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.TicketId);
                });
            migrationBuilder.Sql(@"SET IDENTITY_INSERT [Tickets] ON;
INSERT INTO [Tickets] ([TicketId], [Title], [CreatedAt], [UpdatedAt], [ResolutionDueDate], [ResponseDueDate],
    [CreatedBy], [Status], [Priority], [AssignedTo], [Visibility])
SELECT tm.[NewId], t.[Title], t.[CreatedAt], t.[UpdatedAt], t.[ResolutionDueDate], t.[ResponseDueDate],
    cm.[NewId], t.[Status], t.[Priority], am.[NewId], t.[Visibility]
FROM [Tickets_Old] t
JOIN [__TicketIdMap] tm ON tm.[OldId] = t.[TicketId]
JOIN [__UserIdMap] cm ON cm.[OldId] = t.[CreatedBy]
LEFT JOIN [__UserIdMap] am ON am.[OldId] = t.[AssignedTo];
SET IDENTITY_INSERT [Tickets] OFF;");
            migrationBuilder.Sql("DROP TABLE [Tickets_Old];");

            // 5. Every other user/ticket id column, rewritten in place.
            ConvertColumn(migrationBuilder, "UserProfiles", "UserId", "__UserIdMap", nullable: false);
            ConvertColumn(migrationBuilder, "TicketUpdates", "TicketId", "__TicketIdMap", nullable: false);
            ConvertColumn(migrationBuilder, "TicketUpdates", "UpdatedBy", "__UserIdMap", nullable: false);
            ConvertColumn(migrationBuilder, "TicketLogs", "TicketId", "__TicketIdMap", nullable: false);
            ConvertColumn(migrationBuilder, "TicketLogs", "PerformedBy", "__UserIdMap", nullable: false);
            ConvertColumn(migrationBuilder, "Notifications", "UserId", "__UserIdMap", nullable: false);
            ConvertColumn(migrationBuilder, "Notifications", "TicketId", "__TicketIdMap", nullable: true);
            ConvertColumn(migrationBuilder, "AuditLogs", "TicketId", "__TicketIdMap", nullable: true);
            ConvertColumn(migrationBuilder, "AuditLogs", "UserId", "__UserIdMap", nullable: true);
            ConvertColumn(migrationBuilder, "AuditLogs", "ActorId", "__UserIdMap", nullable: true);
            ConvertColumn(migrationBuilder, "RoundRobinCursors", "LastAssignedUserId", "__UserIdMap", nullable: true);
            ConvertColumn(migrationBuilder, "TicketCreationSagaStates", "TicketId", "__TicketIdMap", nullable: false);
            // RefreshTokens.UserId stays a string (it's the token's claim value), now the int id as text.
            migrationBuilder.Sql(@"UPDATE r SET [UserId] = CAST(m.[NewId] AS nvarchar(20))
FROM [RefreshTokens] r JOIN [__UserIdMap] m ON m.[OldId] = TRY_CONVERT(uniqueidentifier, r.[UserId]);");

            // 6. Keys, indexes and foreign keys back, as the model has them.
            migrationBuilder.AddPrimaryKey(name: "PK_UserProfiles", table: "UserProfiles", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_Tickets_AssignedTo", table: "Tickets", column: "AssignedTo");
            migrationBuilder.CreateIndex(name: "IX_TicketUpdates_TicketId", table: "TicketUpdates", column: "TicketId");
            migrationBuilder.CreateIndex(name: "IX_TicketUpdates_UpdatedBy", table: "TicketUpdates", column: "UpdatedBy");
            migrationBuilder.CreateIndex(name: "IX_Notifications_UserId_IsRead", table: "Notifications", columns: new[] { "UserId", "IsRead" });
            migrationBuilder.CreateIndex(name: "IX_AuditLogs_TicketId_OccurredAtUtc", table: "AuditLogs", columns: new[] { "TicketId", "OccurredAtUtc" });
            migrationBuilder.CreateIndex(name: "IX_AuditLogs_UserId_OccurredAtUtc", table: "AuditLogs", columns: new[] { "UserId", "OccurredAtUtc" });
            migrationBuilder.CreateIndex(name: "IX_TicketCreationSagaStates_TicketId", table: "TicketCreationSagaStates", column: "TicketId", unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Users_UserId",
                table: "UserProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_AssignedTo",
                table: "Tickets",
                column: "AssignedTo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_TicketUpdates_Tickets_TicketId",
                table: "TicketUpdates",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "TicketId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_TicketUpdates_Users_UpdatedBy",
                table: "TicketUpdates",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("DROP TABLE [__UserIdMap];");
            migrationBuilder.Sql("DROP TABLE [__TicketIdMap];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The old Guids aren't kept, so there's nothing to map back to.
            throw new NotSupportedException("ChangeTicketAndUserIdsToInt can't be reverted: the original Guid ids were not kept. Restore a database backup instead.");
        }

        // Swaps a uniqueidentifier column for an int one holding the mapped id (NULL where there's no mapping).
        // Separate Sql calls on purpose: each runs as its own batch, so the new column exists before it's used.
        private static void ConvertColumn(MigrationBuilder migrationBuilder, string table, string column, string map, bool nullable)
        {
            migrationBuilder.Sql($"ALTER TABLE [{table}] ADD [{column}_New] int NULL;");
            migrationBuilder.Sql($"UPDATE t SET [{column}_New] = m.[NewId] FROM [{table}] t JOIN [{map}] m ON m.[OldId] = t.[{column}];");
            migrationBuilder.Sql($"ALTER TABLE [{table}] DROP COLUMN [{column}];");
            migrationBuilder.Sql($"EXEC sp_rename N'[{table}].[{column}_New]', N'{column}', N'COLUMN';");
            if (!nullable)
                migrationBuilder.Sql($"ALTER TABLE [{table}] ALTER COLUMN [{column}] int NOT NULL;");
        }
    }
}
