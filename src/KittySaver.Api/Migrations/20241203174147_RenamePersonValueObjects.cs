using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenamePersonValueObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_ZipCode",
                table: "Persons",
                newName: "ResidentalAddressZipCode");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_Street",
                table: "Persons",
                newName: "ResidentalAddressStreet");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_State",
                table: "Persons",
                newName: "ResidentalAddressState");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_Country",
                table: "Persons",
                newName: "ResidentalAddressCountry");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_City",
                table: "Persons",
                newName: "ResidentalAddressCity");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_BuildingNumber",
                table: "Persons",
                newName: "ResidentalAddressBuildingNumber");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber_Value",
                table: "Persons",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "LastName_Value",
                table: "Persons",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "FirstName_Value",
                table: "Persons",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Email_Value",
                table: "Persons",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddress_ZipCode",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddressZipCode");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddress_Street",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddressStreet");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddress_State",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddressState");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddress_Country",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddressCountry");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddress_City",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddressCity");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddress_BuildingNumber",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddressBuildingNumber");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsContactInfoPhoneNumber_Value",
                table: "Persons",
                newName: "DefaultAdvertisementsContactInfoPhoneNumber");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsContactInfoEmail_Value",
                table: "Persons",
                newName: "DefaultAdvertisementsContactInfoEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResidentalAddressZipCode",
                table: "Persons",
                newName: "ResidentalAddress_ZipCode");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddressStreet",
                table: "Persons",
                newName: "ResidentalAddress_Street");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddressState",
                table: "Persons",
                newName: "ResidentalAddress_State");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddressCountry",
                table: "Persons",
                newName: "ResidentalAddress_Country");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddressCity",
                table: "Persons",
                newName: "ResidentalAddress_City");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddressBuildingNumber",
                table: "Persons",
                newName: "ResidentalAddress_BuildingNumber");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Persons",
                newName: "PhoneNumber_Value");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Persons",
                newName: "LastName_Value");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Persons",
                newName: "FirstName_Value");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Persons",
                newName: "Email_Value");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddressZipCode",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddress_ZipCode");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddressStreet",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddress_Street");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddressState",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddress_State");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddressCountry",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddress_Country");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddressCity",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddress_City");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsPickupAddressBuildingNumber",
                table: "Persons",
                newName: "DefaultAdvertisementsPickupAddress_BuildingNumber");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsContactInfoPhoneNumber",
                table: "Persons",
                newName: "DefaultAdvertisementsContactInfoPhoneNumber_Value");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsContactInfoEmail",
                table: "Persons",
                newName: "DefaultAdvertisementsContactInfoEmail_Value");
        }
    }
}
