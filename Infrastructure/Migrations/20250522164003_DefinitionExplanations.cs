using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class DefinitionExplanations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DefinitionExplanations",
                columns: table => new
                {
                    DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DictionaryDefinitionKey = table.Column<int>(type: "INTEGER", nullable: false),
                    NativeLanguage = table.Column<int>(type: "INTEGER", nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", nullable: false),
                    ExplanationEntryMethod = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefinitionExplanations", x => x.DatabasePrimaryKey);
                    table.ForeignKey(
                        name: "FK_DefinitionExplanations_DictionaryDefinitions_DictionaryDefinitionKey",
                        column: x => x.DictionaryDefinitionKey,
                        principalTable: "DictionaryDefinitions",
                        principalColumn: "DatabasePrimaryKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefinitionExplanations_DictionaryDefinitionKey",
                table: "DefinitionExplanations",
                column: "DictionaryDefinitionKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefinitionExplanations");
        }
    }
}
