using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Auth.Api.Migrations
{
    /// <inheritdoc />
    public partial class DotnetNineUpgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "f581a969-a6e1-44c7-bc00-630b94556ab9", "AQAAAAIAAYagAAAAELDnCnSS1y6NXPsgFQyW6+5hDyDz/sX9Kfj4phlvmD9W+xoZOgBCzSIENV2vAnJDNA==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "17484491-9ad4-47b7-92cb-e0f954050844", "AQAAAAIAAYagAAAAECtrfSbwydETfnrc9jKMiZbUZTQyFEH+mqI6SUW/+h3hTrC0gWRbS8vHlD7BHzURrw==" });
        }
    }
}
