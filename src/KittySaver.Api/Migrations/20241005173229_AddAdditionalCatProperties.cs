using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KittySaver.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalCatProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalRequirements",
                table: "Cat",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgeCategory",
                table: "Cat",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Behavior",
                table: "Cat",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HealthStatus",
                table: "Cat",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCastrated",
                table: "Cat",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInNeedOfSeeingVet",
                table: "Cat",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Cat",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalRequirements",
                table: "Cat");

            migrationBuilder.DropColumn(
                name: "AgeCategory",
                table: "Cat");

            migrationBuilder.DropColumn(
                name: "Behavior",
                table: "Cat");

            migrationBuilder.DropColumn(
                name: "HealthStatus",
                table: "Cat");

            migrationBuilder.DropColumn(
                name: "IsCastrated",
                table: "Cat");

            migrationBuilder.DropColumn(
                name: "IsInNeedOfSeeingVet",
                table: "Cat");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Cat");
        }
    }
}
