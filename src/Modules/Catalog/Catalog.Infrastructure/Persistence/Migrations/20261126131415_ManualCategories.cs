using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ManualCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "ManualCategoryIds",
                schema: "catalog",
                startValue: 800000000L);

            migrationBuilder.AddColumn<bool>(
                name: "IsManual",
                schema: "catalog",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOrderedByHand",
                schema: "catalog",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlacedByHand",
                schema: "catalog",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OneCParentId",
                schema: "catalog",
                table: "ProductCategories",
                type: "integer",
                nullable: true);

            // Until now every category sat where 1C put it: that parent is the 1C parent (spec catalog/012 MC-09).
            migrationBuilder.Sql("""UPDATE catalog."ProductCategories" SET "OneCParentId" = "ParentId";""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManual",
                schema: "catalog",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "IsOrderedByHand",
                schema: "catalog",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "IsPlacedByHand",
                schema: "catalog",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "OneCParentId",
                schema: "catalog",
                table: "ProductCategories");

            migrationBuilder.DropSequence(
                name: "ManualCategoryIds",
                schema: "catalog");
        }
    }
}
