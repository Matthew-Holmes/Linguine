using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class VocalisationFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VocalisedDefinitionFile",
                columns: table => new
                {
                    DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DictionaryDefinitionKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Voice = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocalisedDefinitionFile", x => x.DatabasePrimaryKey);
                    table.ForeignKey(
                        name: "FK_VocalisedDefinitionFile_DictionaryDefinitions_DictionaryDefinitionKey",
                        column: x => x.DictionaryDefinitionKey,
                        principalTable: "DictionaryDefinitions",
                        principalColumn: "DatabasePrimaryKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VocalisedDefinitionFile_DictionaryDefinitionKey",
                table: "VocalisedDefinitionFile",
                column: "DictionaryDefinitionKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VocalisedDefinitionFile");
        }
    }
}
