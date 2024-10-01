using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLastModificationToAuditableEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastModificationBy",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModificationOn",
                table: "Persons",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("a4018ea1-525a-48eb-a701-a96c1a261e72"),
                columns: new[] { "CreatedOn", "LastModificationBy", "LastModificationOn" },
                values: new object[] { new DateTimeOffset(new DateTime(2024, 9, 20, 13, 24, 40, 699, DateTimeKind.Unspecified).AddTicks(1210), new TimeSpan(0, 2, 0, 0, 0)), null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_PhoneNumber",
                table: "Persons",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persons_PhoneNumber",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "LastModificationBy",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "LastModificationOn",
                table: "Persons");

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("a4018ea1-525a-48eb-a701-a96c1a261e72"),
                column: "CreatedOn",
                value: new DateTimeOffset(new DateTime(2024, 8, 16, 0, 5, 40, 250, DateTimeKind.Unspecified).AddTicks(3094), new TimeSpan(0, 2, 0, 0, 0)));
        }
    }
}
