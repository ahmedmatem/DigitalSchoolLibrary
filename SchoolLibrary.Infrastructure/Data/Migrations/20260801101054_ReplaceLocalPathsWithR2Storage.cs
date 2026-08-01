using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolLibrary.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceLocalPathsWithR2Storage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImagePath",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Resources");

            migrationBuilder.AddColumn<string>(
                name: "CoverStorageKey",
                table: "Resources",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileContentType",
                table: "Resources",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Resources",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileStorageKey",
                table: "Resources",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "Resources",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverStorageKey",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "FileContentType",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "FileStorageKey",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "Resources");

            migrationBuilder.AddColumn<string>(
                name: "CoverImagePath",
                table: "Resources",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Resources",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
