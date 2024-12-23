using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Auth.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedApplicationUserAdditionalProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultAdvertisementContactInfoEmail",
                table: "AspNetUsers",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultAdvertisementContactInfoPhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(31)",
                maxLength: 31,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultAdvertisementPickupAddressBuildingNumber",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultAdvertisementPickupAddressCity",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultAdvertisementPickupAddressCountry",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultAdvertisementPickupAddressState",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultAdvertisementPickupAddressStreet",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultAdvertisementPickupAddressZipCode",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"),
                columns: new[] { "ConcurrencyStamp", "DefaultAdvertisementContactInfoEmail", "DefaultAdvertisementContactInfoPhoneNumber", "DefaultAdvertisementPickupAddressBuildingNumber", "DefaultAdvertisementPickupAddressCity", "DefaultAdvertisementPickupAddressCountry", "DefaultAdvertisementPickupAddressState", "DefaultAdvertisementPickupAddressStreet", "DefaultAdvertisementPickupAddressZipCode", "PasswordHash" },
                values: new object[] { "17484491-9ad4-47b7-92cb-e0f954050844", "defaultadmin@koniec.dev", "XXXXXXXXX", "1", "DefaultCity", "DefaultCountry", "DefaultState", "DefaultStreet", "00000", "AQAAAAIAAYagAAAAECtrfSbwydETfnrc9jKMiZbUZTQyFEH+mqI6SUW/+h3hTrC0gWRbS8vHlD7BHzURrw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultAdvertisementContactInfoEmail",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultAdvertisementContactInfoPhoneNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultAdvertisementPickupAddressBuildingNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultAdvertisementPickupAddressCity",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultAdvertisementPickupAddressCountry",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultAdvertisementPickupAddressState",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultAdvertisementPickupAddressStreet",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultAdvertisementPickupAddressZipCode",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "5f2f707f-0f0d-479e-aaf1-111119a3f9ad", "AQAAAAIAAYagAAAAEMrQlexH/LlM+GONZMRdEc7361FKfkve7lBuOkriuCFG7Qu7kjd1xJSLyfm3hHN4Hg==" });
        }
    }
}
