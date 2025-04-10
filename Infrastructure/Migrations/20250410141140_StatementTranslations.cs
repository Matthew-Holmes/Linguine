using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class StatementTranslations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VocalisedDefinitionFile_DictionaryDefinitions_DictionaryDefinitionKey",
                table: "VocalisedDefinitionFile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VocalisedDefinitionFile",
                table: "VocalisedDefinitionFile");

            migrationBuilder.RenameTable(
                name: "VocalisedDefinitionFile",
                newName: "VocalisedDefinitionFiles");

            migrationBuilder.RenameIndex(
                name: "IX_VocalisedDefinitionFile_DictionaryDefinitionKey",
                table: "VocalisedDefinitionFiles",
                newName: "IX_VocalisedDefinitionFiles_DictionaryDefinitionKey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VocalisedDefinitionFiles",
                table: "VocalisedDefinitionFiles",
                column: "DatabasePrimaryKey");

            migrationBuilder.CreateTable(
                name: "TranslatedStatements",
                columns: table => new
                {
                    DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StatementKey = table.Column<int>(type: "INTEGER", nullable: false),
                    TranslatedLanguage = table.Column<int>(type: "INTEGER", nullable: false),
                    Translation = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslatedStatements", x => x.DatabasePrimaryKey);
                    table.ForeignKey(
                        name: "FK_TranslatedStatements_Statements_StatementKey",
                        column: x => x.StatementKey,
                        principalTable: "Statements",
                        principalColumn: "DatabasePrimaryKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TranslatedStatements_StatementKey",
                table: "TranslatedStatements",
                column: "StatementKey");

            migrationBuilder.AddForeignKey(
                name: "FK_VocalisedDefinitionFiles_DictionaryDefinitions_DictionaryDefinitionKey",
                table: "VocalisedDefinitionFiles",
                column: "DictionaryDefinitionKey",
                principalTable: "DictionaryDefinitions",
                principalColumn: "DatabasePrimaryKey",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VocalisedDefinitionFiles_DictionaryDefinitions_DictionaryDefinitionKey",
                table: "VocalisedDefinitionFiles");

            migrationBuilder.DropTable(
                name: "TranslatedStatements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VocalisedDefinitionFiles",
                table: "VocalisedDefinitionFiles");

            migrationBuilder.RenameTable(
                name: "VocalisedDefinitionFiles",
                newName: "VocalisedDefinitionFile");

            migrationBuilder.RenameIndex(
                name: "IX_VocalisedDefinitionFiles_DictionaryDefinitionKey",
                table: "VocalisedDefinitionFile",
                newName: "IX_VocalisedDefinitionFile_DictionaryDefinitionKey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VocalisedDefinitionFile",
                table: "VocalisedDefinitionFile",
                column: "DatabasePrimaryKey");

            migrationBuilder.AddForeignKey(
                name: "FK_VocalisedDefinitionFile_DictionaryDefinitions_DictionaryDefinitionKey",
                table: "VocalisedDefinitionFile",
                column: "DictionaryDefinitionKey",
                principalTable: "DictionaryDefinitions",
                principalColumn: "DatabasePrimaryKey",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
