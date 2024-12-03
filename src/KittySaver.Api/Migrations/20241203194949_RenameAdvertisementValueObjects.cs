using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameAdvertisementValueObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PickupAddress_ZipCode",
                table: "Advertisements",
                newName: "PickupAddressZipCode");

            migrationBuilder.RenameColumn(
                name: "PickupAddress_Street",
                table: "Advertisements",
                newName: "PickupAddressStreet");

            migrationBuilder.RenameColumn(
                name: "PickupAddress_State",
                table: "Advertisements",
                newName: "PickupAddressState");

            migrationBuilder.RenameColumn(
                name: "PickupAddress_Country",
                table: "Advertisements",
                newName: "PickupAddressCountry");

            migrationBuilder.RenameColumn(
                name: "PickupAddress_City",
                table: "Advertisements",
                newName: "PickupAddressCity");

            migrationBuilder.RenameColumn(
                name: "PickupAddress_BuildingNumber",
                table: "Advertisements",
                newName: "PickupAddressBuildingNumber");

            migrationBuilder.RenameColumn(
                name: "Description_Value",
                table: "Advertisements",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "ContactInfoPhoneNumber_Value",
                table: "Advertisements",
                newName: "ContactInfoPhoneNumber");

            migrationBuilder.RenameColumn(
                name: "ContactInfoEmail_Value",
                table: "Advertisements",
                newName: "ContactInfoEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PickupAddressZipCode",
                table: "Advertisements",
                newName: "PickupAddress_ZipCode");

            migrationBuilder.RenameColumn(
                name: "PickupAddressStreet",
                table: "Advertisements",
                newName: "PickupAddress_Street");

            migrationBuilder.RenameColumn(
                name: "PickupAddressState",
                table: "Advertisements",
                newName: "PickupAddress_State");

            migrationBuilder.RenameColumn(
                name: "PickupAddressCountry",
                table: "Advertisements",
                newName: "PickupAddress_Country");

            migrationBuilder.RenameColumn(
                name: "PickupAddressCity",
                table: "Advertisements",
                newName: "PickupAddress_City");

            migrationBuilder.RenameColumn(
                name: "PickupAddressBuildingNumber",
                table: "Advertisements",
                newName: "PickupAddress_BuildingNumber");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Advertisements",
                newName: "Description_Value");

            migrationBuilder.RenameColumn(
                name: "ContactInfoPhoneNumber",
                table: "Advertisements",
                newName: "ContactInfoPhoneNumber_Value");

            migrationBuilder.RenameColumn(
                name: "ContactInfoEmail",
                table: "Advertisements",
                newName: "ContactInfoEmail_Value");
        }
    }
}
