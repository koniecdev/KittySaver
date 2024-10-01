using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KittySaver.Auth.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedRolesAndDefaultAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("86c81ae7-b11c-4343-9577-872a74637200"), null, "Shelter", "Shelter" },
                    { new Guid("b1a6bd96-90d6-4949-b618-ce662f41f87a"), null, "Administrator", "ADMINISTRATOR" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"), 0, "024c9a04-8f34-4c91-9ae8-1d763ec8fc5e", "defaultadmin@koniec.dev", true, "Default", "Admin", false, null, "DEFAULTADMIN@KONIEC.DEV", "DEFAULTADMIN@KONIEC.DEV", "AQAAAAIAAYagAAAAEKUVlaAp9vxi3Gc19ngmEne84gdhAxFqm9wQQEasZ58ADtftN7iDMcq7IfLlTKqTqQ==", "XXXXXXXXX", true, "34d18c72-0a52-4bbe-9b00-754a02e2293e", false, "defaultadmin@koniec.dev" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("86c81ae7-b11c-4343-9577-872a74637200"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("b1a6bd96-90d6-4949-b618-ce662f41f87a"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"));
        }
    }
}
