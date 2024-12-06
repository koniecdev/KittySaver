using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLastNameAndRenameFirstNameToNickname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Persons");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Persons",
                newName: "Nickname");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nickname",
                table: "Persons",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Persons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
