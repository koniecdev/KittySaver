using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Auth.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "UserName" },
                values: new object[] { "5f2f707f-0f0d-479e-aaf1-111119a3f9ad", "AQAAAAIAAYagAAAAEMrQlexH/LlM+GONZMRdEc7361FKfkve7lBuOkriuCFG7Qu7kjd1xJSLyfm3hHN4Hg==", "DefaultAdmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"),
                columns: new[] { "ConcurrencyStamp", "FirstName", "LastName", "PasswordHash", "UserName" },
                values: new object[] { "7919206e-3c54-4acd-9e26-247cfe62f3f9", "Default", "Admin", "AQAAAAIAAYagAAAAEGjIleXwF0TtJ+tZU+yimF24NhElOX/ariLon66C4b8sT+/EZLybTY7utONZXFR4rQ==", "defaultadmin@koniec.dev" });
        }
    }
}
