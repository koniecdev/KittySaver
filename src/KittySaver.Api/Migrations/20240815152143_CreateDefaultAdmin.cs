using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreateDefaultAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserIdentityId", "UserName" },
                values: new object[] { new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"), 0, "7ba7044b-9720-4123-abec-70a2c6686402", "defaultadmin@koniec.dev", true, "Default", "Admin", false, null, "DEFAULTADMIN@KONIEC.DEV", "DEFAULTADMIN@KONIEC.DEV", "AQAAAAIAAYagAAAAEMSI4isQu0fNtl+ss2ulj6nNueR+ciD6GfzKyiLl101NTqjxMDvigehNuoQ43Ufotw==", "XXXXXXXXX", true, "34d18c72-0a52-4bbe-9b00-754a02e2293e", false, new Guid("a4018ea1-525a-48eb-a701-a96c1a261e72"), "defaultadmin@koniec.dev" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"));
        }
    }
}
