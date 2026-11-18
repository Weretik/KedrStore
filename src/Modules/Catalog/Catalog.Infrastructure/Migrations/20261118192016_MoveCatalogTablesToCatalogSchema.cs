using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveCatalogTablesToCatalogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.RenameTable(
                name: "ProductTranslations",
                newName: "ProductTranslations",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Products",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "ProductPrices",
                newName: "ProductPrices",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "ProductMedia",
                newName: "ProductMedia",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "ProductListProjections",
                newName: "ProductListProjections",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "ProductCategories",
                newName: "ProductCategories",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "PriceType",
                newName: "PriceType",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CategoryFaqItems",
                newName: "CategoryFaqItems",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CategoryContents",
                newName: "CategoryContents",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogSlugHistory",
                newName: "CatalogSlugHistory",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogProductGroupTexts",
                newName: "CatalogProductGroupTexts",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogProductGroups",
                newName: "CatalogProductGroups",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogProductGroupMembers",
                newName: "CatalogProductGroupMembers",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogProductGroupAxes",
                newName: "CatalogProductGroupAxes",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogProductAttributeValues",
                newName: "CatalogProductAttributeValues",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogProductAttributeReviews",
                newName: "CatalogProductAttributeReviews",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogCategoryAttributes",
                newName: "CatalogCategoryAttributes",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogAttributeValues",
                newName: "CatalogAttributeValues",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogAttributes",
                newName: "CatalogAttributes",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogAttributeAliasMeanings",
                newName: "CatalogAttributeAliasMeanings",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogAttributeAliases",
                newName: "CatalogAttributeAliases",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "CatalogAttributeAliasCategoryValues",
                newName: "CatalogAttributeAliasCategoryValues",
                newSchema: "catalog");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ProductTranslations",
                schema: "catalog",
                newName: "ProductTranslations");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "catalog",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "ProductPrices",
                schema: "catalog",
                newName: "ProductPrices");

            migrationBuilder.RenameTable(
                name: "ProductMedia",
                schema: "catalog",
                newName: "ProductMedia");

            migrationBuilder.RenameTable(
                name: "ProductListProjections",
                schema: "catalog",
                newName: "ProductListProjections");

            migrationBuilder.RenameTable(
                name: "ProductCategories",
                schema: "catalog",
                newName: "ProductCategories");

            migrationBuilder.RenameTable(
                name: "PriceType",
                schema: "catalog",
                newName: "PriceType");

            migrationBuilder.RenameTable(
                name: "CategoryFaqItems",
                schema: "catalog",
                newName: "CategoryFaqItems");

            migrationBuilder.RenameTable(
                name: "CategoryContents",
                schema: "catalog",
                newName: "CategoryContents");

            migrationBuilder.RenameTable(
                name: "CatalogSlugHistory",
                schema: "catalog",
                newName: "CatalogSlugHistory");

            migrationBuilder.RenameTable(
                name: "CatalogProductGroupTexts",
                schema: "catalog",
                newName: "CatalogProductGroupTexts");

            migrationBuilder.RenameTable(
                name: "CatalogProductGroups",
                schema: "catalog",
                newName: "CatalogProductGroups");

            migrationBuilder.RenameTable(
                name: "CatalogProductGroupMembers",
                schema: "catalog",
                newName: "CatalogProductGroupMembers");

            migrationBuilder.RenameTable(
                name: "CatalogProductGroupAxes",
                schema: "catalog",
                newName: "CatalogProductGroupAxes");

            migrationBuilder.RenameTable(
                name: "CatalogProductAttributeValues",
                schema: "catalog",
                newName: "CatalogProductAttributeValues");

            migrationBuilder.RenameTable(
                name: "CatalogProductAttributeReviews",
                schema: "catalog",
                newName: "CatalogProductAttributeReviews");

            migrationBuilder.RenameTable(
                name: "CatalogCategoryAttributes",
                schema: "catalog",
                newName: "CatalogCategoryAttributes");

            migrationBuilder.RenameTable(
                name: "CatalogAttributeValues",
                schema: "catalog",
                newName: "CatalogAttributeValues");

            migrationBuilder.RenameTable(
                name: "CatalogAttributes",
                schema: "catalog",
                newName: "CatalogAttributes");

            migrationBuilder.RenameTable(
                name: "CatalogAttributeAliasMeanings",
                schema: "catalog",
                newName: "CatalogAttributeAliasMeanings");

            migrationBuilder.RenameTable(
                name: "CatalogAttributeAliases",
                schema: "catalog",
                newName: "CatalogAttributeAliases");

            migrationBuilder.RenameTable(
                name: "CatalogAttributeAliasCategoryValues",
                schema: "catalog",
                newName: "CatalogAttributeAliasCategoryValues");
        }
    }
}
