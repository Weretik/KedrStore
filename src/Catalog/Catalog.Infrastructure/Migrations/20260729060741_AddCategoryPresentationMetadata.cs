using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryPresentationMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "ProductCategories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ShortNameRu",
                table: "ProductCategories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortNameUk",
                table: "ProductCategories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ProductCategories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "ProductCategories"
                SET "ShortNameUk" = "Name",
                    "ShortNameRu" = "Name",
                    "SortOrder" = 2147483647,
                    "Level" = nlevel("Path") - 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentId_SortOrder_Id",
                table: "ProductCategories",
                columns: new[] { "ParentId", "SortOrder", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_ParentId_SortOrder_Id",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ShortNameRu",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ShortNameUk",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "ProductCategories");
        }
    }
}
