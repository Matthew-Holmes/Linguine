using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class RemoveMultipleSources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "DictionaryDefinitions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Variants",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "DictionaryDefinitions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
