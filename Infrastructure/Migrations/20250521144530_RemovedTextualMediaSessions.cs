using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class RemovedTextualMediaSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TextualMediaSessions");

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "TextualMedia",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActive",
                table: "TextualMedia",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "TextualMedia");

            migrationBuilder.DropColumn(
                name: "LastActive",
                table: "TextualMedia");

            migrationBuilder.CreateTable(
                name: "TextualMediaSessions",
                columns: table => new
                {
                    DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TextualMediaKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    Cursor = table.Column<int>(type: "INTEGER", nullable: false),
                    LastActive = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextualMediaSessions", x => x.DatabasePrimaryKey);
                    table.ForeignKey(
                        name: "FK_TextualMediaSessions_TextualMedia_TextualMediaKey",
                        column: x => x.TextualMediaKey,
                        principalTable: "TextualMedia",
                        principalColumn: "DatabasePrimaryKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TextualMediaSessions_TextualMediaKey",
                table: "TextualMediaSessions",
                column: "TextualMediaKey");
        }
    }
}
