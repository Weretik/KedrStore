using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CategorySearchSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClosedFromSearch",
                schema: "catalog",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ShareImageMediaFileId",
                schema: "catalog",
                table: "ProductCategories",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteNameRu",
                schema: "catalog",
                table: "ProductCategories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteNameUk",
                schema: "catalog",
                table: "ProductCategories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClosedFromSearch",
                schema: "catalog",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ShareImageMediaFileId",
                schema: "catalog",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "SiteNameRu",
                schema: "catalog",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "SiteNameUk",
                schema: "catalog",
                table: "ProductCategories");
        }
    }
}
