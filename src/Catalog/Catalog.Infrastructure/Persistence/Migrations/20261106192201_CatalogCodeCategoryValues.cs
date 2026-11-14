using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogCodeCategoryValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogAttributeAliasCategoryValues",
                columns: table => new
                {
                    AliasId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    ValueId = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogAttributeAliasCategoryValues", x => new { x.AliasId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_CatalogAttributeAliasCategoryValues_CatalogAttributeAliases~",
                        column: x => x.AliasId,
                        principalTable: "CatalogAttributeAliases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeAliasCategoryValues_CatalogAttributeValues_~",
                        column: x => x.ValueId,
                        principalTable: "CatalogAttributeValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeAliasCategoryValues_ProductCategories_Categ~",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeAliasCategoryValues_CategoryId",
                table: "CatalogAttributeAliasCategoryValues",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeAliasCategoryValues_ValueId",
                table: "CatalogAttributeAliasCategoryValues",
                column: "ValueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogAttributeAliasCategoryValues");
        }
    }
}
