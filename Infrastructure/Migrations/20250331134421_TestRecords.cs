using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class TestRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "TestRecords",
            //    columns: table => new
            //    {
            //        DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        DictionaryDefinitionKey = table.Column<int>(type: "INTEGER", nullable: false),
            //        Posed = table.Column<DateTime>(type: "TEXT", nullable: false),
            //        Answered = table.Column<DateTime>(type: "TEXT", nullable: false),
            //        Finished = table.Column<DateTime>(type: "TEXT", nullable: false),
            //        Correct = table.Column<bool>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TestRecords", x => x.DatabasePrimaryKey);
            //        table.ForeignKey(
            //            name: "FK_TestRecords_DictionaryDefinitions_DictionaryDefinitionKey",
            //            column: x => x.DictionaryDefinitionKey,
            //            principalTable: "DictionaryDefinitions",
            //            principalColumn: "DatabasePrimaryKey",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_TestRecords_DictionaryDefinitionKey",
            //    table: "TestRecords",
            //    column: "DictionaryDefinitionKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestRecords");
        }
    }
}
