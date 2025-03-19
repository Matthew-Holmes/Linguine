using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // uncomment if no existing database - i.e. implementing a new language

            //migrationBuilder.CreateTable(
            //    name: "DictionaryDefinitions",
            //    columns: table => new
            //    {
            //        DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        ID = table.Column<int>(type: "INTEGER", nullable: false),
            //        Word = table.Column<string>(type: "TEXT", nullable: false),
            //        Definition = table.Column<string>(type: "TEXT", nullable: false),
            //        Source = table.Column<string>(type: "TEXT", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DictionaryDefinitions", x => x.DatabasePrimaryKey);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "TextualMedia",
            //    columns: table => new
            //    {
            //        DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        Name = table.Column<string>(type: "TEXT", nullable: false),
            //        Text = table.Column<string>(type: "TEXT", nullable: false),
            //        Description = table.Column<string>(type: "TEXT", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TextualMedia", x => x.DatabasePrimaryKey);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Variants",
            //    columns: table => new
            //    {
            //        DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        Variant = table.Column<string>(type: "TEXT", nullable: false),
            //        Root = table.Column<string>(type: "TEXT", nullable: false),
            //        Source = table.Column<string>(type: "TEXT", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Variants", x => x.DatabasePrimaryKey);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ParsedDictionaryDefinitions",
            //    columns: table => new
            //    {
            //        DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        DictionaryDefinitionKey = table.Column<int>(type: "INTEGER", nullable: false),
            //        LearnerLevel = table.Column<int>(type: "INTEGER", nullable: false),
            //        NativeLanguage = table.Column<int>(type: "INTEGER", nullable: false),
            //        ParsedDefinition = table.Column<string>(type: "TEXT", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ParsedDictionaryDefinitions", x => x.DatabasePrimaryKey);
            //        table.ForeignKey(
            //            name: "FK_ParsedDictionaryDefinitions_DictionaryDefinitions_DictionaryDefinitionKey",
            //            column: x => x.DictionaryDefinitionKey,
            //            principalTable: "DictionaryDefinitions",
            //            principalColumn: "DatabasePrimaryKey",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Statements",
            //    columns: table => new
            //    {
            //        DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        ParentKey = table.Column<int>(type: "INTEGER", nullable: false),
            //        FirstCharIndex = table.Column<int>(type: "INTEGER", nullable: false),
            //        LastCharIndex = table.Column<int>(type: "INTEGER", nullable: false),
            //        PreviousKey = table.Column<int>(type: "INTEGER", nullable: true),
            //        ContextCheckpoint = table.Column<string>(type: "TEXT", nullable: true),
            //        ContextDeltaRemovalsDescendingIndex = table.Column<string>(type: "TEXT", nullable: false),
            //        ContextDeltaInsertionsDescendingIndex = table.Column<string>(type: "TEXT", nullable: false),
            //        HeadlessInjectiveDecompositionJSON = table.Column<string>(type: "TEXT", nullable: false),
            //        HeadlessRootedDecompositionJSON = table.Column<string>(type: "TEXT", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Statements", x => x.DatabasePrimaryKey);
            //        table.ForeignKey(
            //            name: "FK_Statements_Statements_PreviousKey",
            //            column: x => x.PreviousKey,
            //            principalTable: "Statements",
            //            principalColumn: "DatabasePrimaryKey");
            //        table.ForeignKey(
            //            name: "FK_Statements_TextualMedia_ParentKey",
            //            column: x => x.ParentKey,
            //            principalTable: "TextualMedia",
            //            principalColumn: "DatabasePrimaryKey",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "TextualMediaSessions",
            //    columns: table => new
            //    {
            //        DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        TextualMediaKey = table.Column<int>(type: "INTEGER", nullable: false),
            //        Cursor = table.Column<int>(type: "INTEGER", nullable: false),
            //        Active = table.Column<bool>(type: "INTEGER", nullable: false),
            //        LastActive = table.Column<DateTime>(type: "TEXT", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TextualMediaSessions", x => x.DatabasePrimaryKey);
            //        table.ForeignKey(
            //            name: "FK_TextualMediaSessions_TextualMedia_TextualMediaKey",
            //            column: x => x.TextualMediaKey,
            //            principalTable: "TextualMedia",
            //            principalColumn: "DatabasePrimaryKey",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "StatementDefinitions",
            //    columns: table => new
            //    {
            //        DatabasePrimaryKey = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        CurrentLevel = table.Column<int>(type: "INTEGER", nullable: false),
            //        IndexAtCurrentLevel = table.Column<int>(type: "INTEGER", nullable: false),
            //        DefinitionKey = table.Column<int>(type: "INTEGER", nullable: false),
            //        StatementKey = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_StatementDefinitions", x => x.DatabasePrimaryKey);
            //        table.ForeignKey(
            //            name: "FK_StatementDefinitions_DictionaryDefinitions_DefinitionKey",
            //            column: x => x.DefinitionKey,
            //            principalTable: "DictionaryDefinitions",
            //            principalColumn: "DatabasePrimaryKey",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_StatementDefinitions_Statements_StatementKey",
            //            column: x => x.StatementKey,
            //            principalTable: "Statements",
            //            principalColumn: "DatabasePrimaryKey",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_ParsedDictionaryDefinitions_DictionaryDefinitionKey",
            //    table: "ParsedDictionaryDefinitions",
            //    column: "DictionaryDefinitionKey");

            //migrationBuilder.CreateIndex(
            //    name: "IX_StatementDefinitions_DefinitionKey",
            //    table: "StatementDefinitions",
            //    column: "DefinitionKey");

            //migrationBuilder.CreateIndex(
            //    name: "IX_StatementDefinitions_StatementKey",
            //    table: "StatementDefinitions",
            //    column: "StatementKey");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Statements_ParentKey",
            //    table: "Statements",
            //    column: "ParentKey");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Statements_PreviousKey",
            //    table: "Statements",
            //    column: "PreviousKey",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_TextualMediaSessions_TextualMediaKey",
            //    table: "TextualMediaSessions",
            //    column: "TextualMediaKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParsedDictionaryDefinitions");

            migrationBuilder.DropTable(
                name: "StatementDefinitions");

            migrationBuilder.DropTable(
                name: "TextualMediaSessions");

            migrationBuilder.DropTable(
                name: "Variants");

            migrationBuilder.DropTable(
                name: "DictionaryDefinitions");

            migrationBuilder.DropTable(
                name: "Statements");

            migrationBuilder.DropTable(
                name: "TextualMedia");
        }
    }
}
