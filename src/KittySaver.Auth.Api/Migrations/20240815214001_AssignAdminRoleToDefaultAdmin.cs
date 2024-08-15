using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Auth.Api.Migrations
{
    /// <inheritdoc />
    public partial class AssignAdminRoleToDefaultAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("b1a6bd96-90d6-4949-b618-ce662f41f87a"), new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424") });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "7919206e-3c54-4acd-9e26-247cfe62f3f9", "AQAAAAIAAYagAAAAEGjIleXwF0TtJ+tZU+yimF24NhElOX/ariLon66C4b8sT+/EZLybTY7utONZXFR4rQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("b1a6bd96-90d6-4949-b618-ce662f41f87a"), new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424") });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "024c9a04-8f34-4c91-9ae8-1d763ec8fc5e", "AQAAAAIAAYagAAAAEKUVlaAp9vxi3Gc19ngmEne84gdhAxFqm9wQQEasZ58ADtftN7iDMcq7IfLlTKqTqQ==" });
        }
    }
}
