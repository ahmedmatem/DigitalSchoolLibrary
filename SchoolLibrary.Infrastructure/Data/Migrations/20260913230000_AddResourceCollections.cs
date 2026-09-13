using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolLibrary.Infrastructure.Data.Migrations
{
    public partial class AddResourceCollections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

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

            migrationBuilder.AlterColumn<Guid>(
                name: "SubjectId",
                table: "Resources",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

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

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CollectionType_Name",
                table: "Categories",
                columns: new[] { "CollectionType", "Name" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Resources_CollectionSubject",
                table: "Resources",
                sql: "([CollectionType] = 1 AND [SubjectId] IS NOT NULL) OR ([CollectionType] = 2 AND [SubjectId] IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Resources_ELibraryVisibility",
                table: "Resources",
                sql: "[CollectionType] <> 2 OR ([AudienceType] = 1 AND [IsPubliclyVisible] = 1)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Resources_CollectionSubject",
                table: "Resources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Resources_ELibraryVisibility",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_CollectionType",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CollectionType_Name",
                table: "Categories");

            migrationBuilder.Sql("DELETE FROM [Resources] WHERE [CollectionType] = 2");

            migrationBuilder.Sql(
                "DELETE FROM [Categories] WHERE [CollectionType] = 2");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubjectId",
                table: "Resources",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "CollectionType",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "CollectionType",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);
        }
    }
}
