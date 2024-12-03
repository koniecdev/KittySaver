using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameCatValueObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name_Value",
                table: "Cats",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "AdditionalRequirements_Value",
                table: "Cats",
                newName: "AdditionalRequirements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Cats",
                newName: "Name_Value");

            migrationBuilder.RenameColumn(
                name: "AdditionalRequirements",
                table: "Cats",
                newName: "AdditionalRequirements_Value");
        }
    }
}
