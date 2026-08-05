using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolLibrary.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModerationStatus",
                table: "Resources",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Resources",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "Resources",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "Resources",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAtUtc",
                table: "Resources",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedByUserId",
                table: "Resources",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ModerationStatus",
                table: "Resources",
                column: "ModerationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ReviewedByUserId",
                table: "Resources",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_SubmittedByUserId",
                table: "Resources",
                column: "SubmittedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_AspNetUsers_ReviewedByUserId",
                table: "Resources",
                column: "ReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_AspNetUsers_SubmittedByUserId",
                table: "Resources",
                column: "SubmittedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_AspNetUsers_ReviewedByUserId",
                table: "Resources");

            migrationBuilder.DropForeignKey(
                name: "FK_Resources_AspNetUsers_SubmittedByUserId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_ModerationStatus",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_ReviewedByUserId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_SubmittedByUserId",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "SubmittedAtUtc",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "Resources");
        }
    }
}
