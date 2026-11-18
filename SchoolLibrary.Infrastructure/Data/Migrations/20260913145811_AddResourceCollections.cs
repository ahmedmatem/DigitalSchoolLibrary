using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SchoolLibrary.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubjectId",
                table: "Resources",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "ModerationStatus",
                table: "Resources",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "CollectionType",
                table: "Resources",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CollectionType",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CollectionType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                column: "CollectionType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CollectionType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "CollectionType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                column: "CollectionType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                column: "CollectionType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                column: "CollectionType",
                value: 1);

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CollectionType", "Name" },
                values: new object[,]
                {
                    { new Guid("e1000000-0000-0000-0000-000000000001"), 2, "Художествена литература" },
                    { new Guid("e1000000-0000-0000-0000-000000000002"), 2, "Поезия" },
                    { new Guid("e1000000-0000-0000-0000-000000000003"), 2, "Статии и публикации" },
                    { new Guid("e1000000-0000-0000-0000-000000000004"), 2, "Научнопопулярна литература" },
                    { new Guid("e1000000-0000-0000-0000-000000000005"), 2, "Списания" },
                    { new Guid("e1000000-0000-0000-0000-000000000006"), 2, "Справочна литература" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CollectionType",
                table: "Resources",
                column: "CollectionType");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Resources_CollectionSubject",
                table: "Resources",
                sql: "([CollectionType] = 1 AND [SubjectId] IS NOT NULL) OR ([CollectionType] = 2 AND [SubjectId] IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Resources_ELibraryVisibility",
                table: "Resources",
                sql: "[CollectionType] <> 2 OR ([AudienceType] = 1 AND [IsPubliclyVisible] = 1)");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CollectionType_Name",
                table: "Categories",
                columns: new[] { "CollectionType", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Resources_CollectionType",
                table: "Resources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Resources_CollectionSubject",
                table: "Resources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Resources_ELibraryVisibility",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CollectionType_Name",
                table: "Categories");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e1000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e1000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e1000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e1000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e1000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e1000000-0000-0000-0000-000000000006"));

            migrationBuilder.DropColumn(
                name: "CollectionType",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "CollectionType",
                table: "Categories");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubjectId",
                table: "Resources",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ModerationStatus",
                table: "Resources",
                type: "int",
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);
        }
    }
}
