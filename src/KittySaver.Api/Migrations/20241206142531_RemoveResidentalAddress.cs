using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveResidentalAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResidentalAddressBuildingNumber",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "ResidentalAddressCity",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "ResidentalAddressCountry",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "ResidentalAddressState",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "ResidentalAddressStreet",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "ResidentalAddressZipCode",
                table: "Persons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResidentalAddressBuildingNumber",
                table: "Persons",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResidentalAddressCity",
                table: "Persons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResidentalAddressCountry",
                table: "Persons",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResidentalAddressState",
                table: "Persons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidentalAddressStreet",
                table: "Persons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResidentalAddressZipCode",
                table: "Persons",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
