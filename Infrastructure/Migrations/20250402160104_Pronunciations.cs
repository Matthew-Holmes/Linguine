using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Pronunciations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IPAPronunciation",
                table: "DictionaryDefinitions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RomanisedPronuncation",
                table: "DictionaryDefinitions",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IPAPronunciation",
                table: "DictionaryDefinitions");

            migrationBuilder.DropColumn(
                name: "RomanisedPronuncation",
                table: "DictionaryDefinitions");
        }
    }
}
