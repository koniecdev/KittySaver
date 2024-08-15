using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreateRoles : Migration
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
        }
    }
}
