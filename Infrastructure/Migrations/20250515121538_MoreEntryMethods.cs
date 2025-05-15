using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class MoreEntryMethods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParsedDefinitionEntryMethod",
                table: "ParsedDictionaryDefinitions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefinitionEntryMethod",
                table: "DictionaryDefinitions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IPAEntryMethod",
                table: "DictionaryDefinitions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RomanisedEntryMethod",
                table: "DictionaryDefinitions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParsedDefinitionEntryMethod",
                table: "ParsedDictionaryDefinitions");

            migrationBuilder.DropColumn(
                name: "DefinitionEntryMethod",
                table: "DictionaryDefinitions");

            migrationBuilder.DropColumn(
                name: "IPAEntryMethod",
                table: "DictionaryDefinitions");

            migrationBuilder.DropColumn(
                name: "RomanisedEntryMethod",
                table: "DictionaryDefinitions");
        }
    }
}
