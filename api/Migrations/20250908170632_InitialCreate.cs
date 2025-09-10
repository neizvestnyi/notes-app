using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NotesApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Notes",
                columns: new[] { "Id", "Content", "CreatedAtUtc", "Title", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8"), "1. Build a task management system\n2. Create a recipe sharing platform\n3. Develop a fitness tracker", new DateTime(2025, 9, 8, 17, 6, 32, 556, DateTimeKind.Utc).AddTicks(3600), "Project Ideas", new DateTime(2025, 9, 8, 17, 6, 32, 556, DateTimeKind.Utc).AddTicks(3600) },
                    { new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"), "This is your first note. Feel free to edit or delete it.", new DateTime(2025, 9, 8, 17, 6, 32, 556, DateTimeKind.Utc).AddTicks(3280), "Welcome to Notes App", new DateTime(2025, 9, 8, 17, 6, 32, 556, DateTimeKind.Utc).AddTicks(3440) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes");
        }
    }
}
