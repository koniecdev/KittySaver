using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class MigrationsSquashed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentRole = table.Column<int>(type: "int", nullable: false),
                    UserIdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultAdvertisementsContactInfoEmail_Value = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DefaultAdvertisementsContactInfoPhoneNumber_Value = table.Column<string>(type: "nvarchar(31)", maxLength: 31, nullable: false),
                    DefaultAdvertisementsPickupAddress_BuildingNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultAdvertisementsPickupAddress_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultAdvertisementsPickupAddress_Country = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DefaultAdvertisementsPickupAddress_State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultAdvertisementsPickupAddress_Street = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultAdvertisementsPickupAddress_ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email_Value = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName_Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName_Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber_Value = table.Column<string>(type: "nvarchar(31)", maxLength: 31, nullable: false),
                    ResidentalAddress_BuildingNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResidentalAddress_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResidentalAddress_Country = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ResidentalAddress_State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResidentalAddress_Street = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResidentalAddress_ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModificationBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModificationOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Advertisements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClosedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExpiresOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PriorityScore = table.Column<double>(type: "float", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactInfoEmail_Value = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContactInfoPhoneNumber_Value = table.Column<string>(type: "nvarchar(31)", maxLength: 31, nullable: false),
                    Description_Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PickupAddress_BuildingNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PickupAddress_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PickupAddress_Country = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    PickupAddress_State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PickupAddress_Street = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PickupAddress_ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModificationBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModificationOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advertisements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Advertisements_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdvertisementId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MedicalHelpUrgency = table.Column<int>(type: "int", nullable: false),
                    AgeCategory = table.Column<int>(type: "int", nullable: false),
                    Behavior = table.Column<int>(type: "int", nullable: false),
                    HealthStatus = table.Column<int>(type: "int", nullable: false),
                    IsCastrated = table.Column<bool>(type: "bit", nullable: false),
                    IsAdopted = table.Column<bool>(type: "bit", nullable: false),
                    PriorityScore = table.Column<double>(type: "float", nullable: false),
                    AdditionalRequirements_Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Name_Value = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModificationBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModificationOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cats_Advertisements_AdvertisementId",
                        column: x => x.AdvertisementId,
                        principalTable: "Advertisements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cats_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_PersonId",
                table: "Advertisements",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Cats_AdvertisementId",
                table: "Cats",
                column: "AdvertisementId");

            migrationBuilder.CreateIndex(
                name: "IX_Cats_PersonId",
                table: "Cats",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cats");

            migrationBuilder.DropTable(
                name: "Advertisements");

            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
