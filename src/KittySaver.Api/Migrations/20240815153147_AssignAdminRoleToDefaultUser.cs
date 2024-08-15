using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class AssignAdminRoleToDefaultUser : Migration
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
                values: new object[] { "1e6862c5-7298-4bad-adfd-29672c4854cb", "AQAAAAIAAYagAAAAELAqGLMmIUMpGoIZQYw5x50MRPa24yJfCPJ7MP4xbyvgjo0VmTlcbfFudgeGx1wIoQ==" });
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
                values: new object[] { "7ba7044b-9720-4123-abec-70a2c6686402", "AQAAAAIAAYagAAAAEMSI4isQu0fNtl+ss2ulj6nNueR+ciD6GfzKyiLl101NTqjxMDvigehNuoQ43Ufotw==" });
        }
    }
}
