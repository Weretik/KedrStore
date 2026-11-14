using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CategorySiteVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShownOnSite",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // One-time start (spec catalog/007 R-18, owner 2026-11-05): everything in the hardware root «Фурнітура»
            // (1C 000005513) is shown on the site; doors, Cosmos, and the rest stay hidden until an admin shows them.
            migrationBuilder.Sql(
                """UPDATE "ProductCategories" SET "IsShownOnSite" = TRUE WHERE "ProductTypeIdOneC" = '000005513';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsShownOnSite",
                table: "ProductCategories");
        }
    }
}
