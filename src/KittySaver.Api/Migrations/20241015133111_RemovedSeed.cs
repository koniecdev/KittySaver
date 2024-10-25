using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemovedSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cat_Persons_PersonId",
                table: "Cat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cat",
                table: "Cat");

            migrationBuilder.DeleteData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("a4018ea1-525a-48eb-a701-a96c1a261e72"));

            migrationBuilder.RenameTable(
                name: "Cat",
                newName: "Cats");

            migrationBuilder.RenameIndex(
                name: "IX_Cat_PersonId",
                table: "Cats",
                newName: "IX_Cats_PersonId");

            migrationBuilder.AddColumn<string>(
                name: "Address_BuildingNumber",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_State",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_ZipCode",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalRequirements",
                table: "Cats",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedicalHelpUrgency",
                table: "Cats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cats",
                table: "Cats",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cats_Persons_PersonId",
                table: "Cats",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cats_Persons_PersonId",
                table: "Cats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cats",
                table: "Cats");

            migrationBuilder.DropColumn(
                name: "Address_BuildingNumber",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Address_State",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Address_ZipCode",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "MedicalHelpUrgency",
                table: "Cats");

            migrationBuilder.RenameTable(
                name: "Cats",
                newName: "Cat");

            migrationBuilder.RenameIndex(
                name: "IX_Cats_PersonId",
                table: "Cat",
                newName: "IX_Cat_PersonId");

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalRequirements",
                table: "Cat",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cat",
                table: "Cat",
                column: "Id");

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "CurrentRole", "Email", "FirstName", "LastModificationBy", "LastModificationOn", "LastName", "PhoneNumber", "UserIdentityId" },
                values: new object[] { new Guid("a4018ea1-525a-48eb-a701-a96c1a261e72"), "", new DateTimeOffset(new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2, "defaultadmin@koniec.dev", "Default", null, null, "Admin", "XXXXXXXXX", new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424") });

            migrationBuilder.AddForeignKey(
                name: "FK_Cat_Persons_PersonId",
                table: "Cat",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
