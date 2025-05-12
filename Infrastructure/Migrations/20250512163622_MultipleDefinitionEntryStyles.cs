using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class MultipleDefinitionEntryStyles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WasManuallyEntered",
                table: "StatementDefinitions",
                newName: "EntryMethod");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EntryMethod",
                table: "StatementDefinitions",
                newName: "WasManuallyEntered");
        }
    }
}
