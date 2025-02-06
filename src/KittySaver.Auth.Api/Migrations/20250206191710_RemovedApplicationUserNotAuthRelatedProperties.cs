using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Auth.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemovedApplicationUserNotAuthRelatedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                columns: new[] { "DefaultAdvertisementContactInfoEmail", "DefaultAdvertisementContactInfoPhoneNumber", "DefaultAdvertisementPickupAddressBuildingNumber", "DefaultAdvertisementPickupAddressCity", "DefaultAdvertisementPickupAddressCountry", "DefaultAdvertisementPickupAddressState", "DefaultAdvertisementPickupAddressStreet", "DefaultAdvertisementPickupAddressZipCode" },
                values: new object[] { "defaultadmin@koniec.dev", "XXXXXXXXX", "1", "DefaultCity", "DefaultCountry", "DefaultState", "DefaultStreet", "00000" });
        }
    }
}
