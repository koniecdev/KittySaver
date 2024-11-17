using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class DomainRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persons_Email",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_PhoneNumber",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_UserIdentityId",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "DefaultAdvertisementsContactInfo_Email",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "AdditionalRequirements",
                table: "Cats");

            migrationBuilder.DropColumn(
                name: "IsInNeedOfSeeingVet",
                table: "Cats");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Cats");

            migrationBuilder.DropColumn(
                name: "ContactInfo_Email",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Advertisements");

            migrationBuilder.RenameColumn(
                name: "Address_ZipCode",
                table: "Persons",
                newName: "ResidentalAddress_ZipCode");

            migrationBuilder.RenameColumn(
                name: "Address_Street",
                table: "Persons",
                newName: "ResidentalAddress_Street");

            migrationBuilder.RenameColumn(
                name: "Address_State",
                table: "Persons",
                newName: "ResidentalAddress_State");

            migrationBuilder.RenameColumn(
                name: "Address_Country",
                table: "Persons",
                newName: "ResidentalAddress_Country");

            migrationBuilder.RenameColumn(
                name: "Address_City",
                table: "Persons",
                newName: "ResidentalAddress_City");

            migrationBuilder.RenameColumn(
                name: "Address_BuildingNumber",
                table: "Persons",
                newName: "ResidentalAddress_BuildingNumber");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Persons",
                newName: "PhoneNumber_Value");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsContactInfo_PhoneNumber",
                table: "Persons",
                newName: "DefaultAdvertisementsContactInfoPhoneNumber_Value");

            migrationBuilder.RenameColumn(
                name: "ContactInfo_PhoneNumber",
                table: "Advertisements",
                newName: "ContactInfoPhoneNumber_Value");

            migrationBuilder.AlterColumn<string>(
                name: "DefaultAdvertisementsPickupAddress_Street",
                table: "Persons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DefaultAdvertisementsPickupAddress_BuildingNumber",
                table: "Persons",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultAdvertisementsContactInfoEmail_Value",
                table: "Persons",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email_Value",
                table: "Persons",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName_Value",
                table: "Persons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName_Value",
                table: "Persons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalRequirements_Value",
                table: "Cats",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name_Value",
                table: "Cats",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PickupAddress_Street",
                table: "Advertisements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PickupAddress_BuildingNumber",
                table: "Advertisements",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactInfoEmail_Value",
                table: "Advertisements",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description_Value",
                table: "Advertisements",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultAdvertisementsContactInfoEmail_Value",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Email_Value",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "FirstName_Value",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "LastName_Value",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "AdditionalRequirements_Value",
                table: "Cats");

            migrationBuilder.DropColumn(
                name: "Name_Value",
                table: "Cats");

            migrationBuilder.DropColumn(
                name: "ContactInfoEmail_Value",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Description_Value",
                table: "Advertisements");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_ZipCode",
                table: "Persons",
                newName: "Address_ZipCode");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_Street",
                table: "Persons",
                newName: "Address_Street");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_State",
                table: "Persons",
                newName: "Address_State");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_Country",
                table: "Persons",
                newName: "Address_Country");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_City",
                table: "Persons",
                newName: "Address_City");

            migrationBuilder.RenameColumn(
                name: "ResidentalAddress_BuildingNumber",
                table: "Persons",
                newName: "Address_BuildingNumber");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber_Value",
                table: "Persons",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "DefaultAdvertisementsContactInfoPhoneNumber_Value",
                table: "Persons",
                newName: "DefaultAdvertisementsContactInfo_PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "ContactInfoPhoneNumber_Value",
                table: "Advertisements",
                newName: "ContactInfo_PhoneNumber");

            migrationBuilder.AlterColumn<string>(
                name: "DefaultAdvertisementsPickupAddress_Street",
                table: "Persons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DefaultAdvertisementsPickupAddress_BuildingNumber",
                table: "Persons",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "DefaultAdvertisementsContactInfo_Email",
                table: "Persons",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Persons",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Persons",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Persons",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalRequirements",
                table: "Cats",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInNeedOfSeeingVet",
                table: "Cats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Cats",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PickupAddress_Street",
                table: "Advertisements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "PickupAddress_BuildingNumber",
                table: "Advertisements",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo_Email",
                table: "Advertisements",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Advertisements",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Email",
                table: "Persons",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_PhoneNumber",
                table: "Persons",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_UserIdentityId",
                table: "Persons",
                column: "UserIdentityId",
                unique: true);
        }
    }
}
