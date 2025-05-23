using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ExplanationComponents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExplanationEntryMethod",
                table: "DefinitionExplanations",
                newName: "SynonymsEntryMethod");

            migrationBuilder.RenameColumn(
                name: "Explanation",
                table: "DefinitionExplanations",
                newName: "Synonyms");

            migrationBuilder.AddColumn<string>(
                name: "DetailedDefinition",
                table: "DefinitionExplanations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DetailedDefinitionEntryMethod",
                table: "DefinitionExplanations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DifferencesEntryMethod",
                table: "DefinitionExplanations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DifferencesFromOtherSenses",
                table: "DefinitionExplanations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Example1",
                table: "DefinitionExplanations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Example1EntryMethod",
                table: "DefinitionExplanations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Example2",
                table: "DefinitionExplanations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Example2EntryMethod",
                table: "DefinitionExplanations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Example3",
                table: "DefinitionExplanations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Example3EntryMethod",
                table: "DefinitionExplanations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Register",
                table: "DefinitionExplanations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RegisterEntryMethod",
                table: "DefinitionExplanations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailedDefinition",
                table: "DefinitionExplanations");

            migrationBuilder.DropColumn(
                name: "DetailedDefinitionEntryMethod",
                table: "DefinitionExplanations");

            migrationBuilder.DropColumn(
                name: "DifferencesEntryMethod",
                table: "DefinitionExplanations");

            migrationBuilder.DropColumn(
                name: "DifferencesFromOtherSenses",
                table: "DefinitionExplanations");

            migrationBuilder.DropColumn(
                name: "Example1",
                table: "DefinitionExplanations");

            migrationBuilder.DropColumn(
                name: "Example1EntryMethod",
                table: "DefinitionExplanations");

            migrationBuilder.DropColumn(
                name: "Example2",
                table: "DefinitionExplanations");

            migrationBuilder.DropColumn(
                name: "Example2EntryMethod",
                table: "DefinitionExplanations");

            migrationBuilder.DropColumn(
                name: "Example3",
                table: "DefinitionExplanations");

            migrationBuilder.DropColumn(
                name: "Example3EntryMethod",
                table: "DefinitionExplanations");

            migrationBuilder.DropColumn(
                name: "Register",
                table: "DefinitionExplanations");

            migrationBuilder.DropColumn(
                name: "RegisterEntryMethod",
                table: "DefinitionExplanations");

            migrationBuilder.RenameColumn(
                name: "SynonymsEntryMethod",
                table: "DefinitionExplanations",
                newName: "ExplanationEntryMethod");

            migrationBuilder.RenameColumn(
                name: "Synonyms",
                table: "DefinitionExplanations",
                newName: "Explanation");
        }
    }
}
