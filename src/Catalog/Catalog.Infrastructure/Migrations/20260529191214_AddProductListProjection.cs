using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductListProjection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductListProjections",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    NameUk = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NameRu = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ProductSlug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Photo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    InStock = table.Column<bool>(type: "boolean", nullable: false),
                    IsSale = table.Column<bool>(type: "boolean", nullable: false),
                    IsNew = table.Column<bool>(type: "boolean", nullable: false),
                    RetailPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SearchTextUk = table.Column<string>(type: "character varying(700)", maxLength: 700, nullable: false),
                    SearchTextRu = table.Column<string>(type: "character varying(700)", maxLength: 700, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductListProjections", x => x.ProductId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductListProjections_CategoryId",
                table: "ProductListProjections",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductListProjections_ListFilters",
                table: "ProductListProjections",
                columns: new[] { "CategoryId", "InStock", "IsSale", "IsNew" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductListProjections_RetailPrice",
                table: "ProductListProjections",
                column: "RetailPrice");

            migrationBuilder.CreateIndex(
                name: "IX_ProductListProjections_SearchTextRu_trgm",
                table: "ProductListProjections",
                column: "SearchTextRu")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductListProjections_SearchTextUk_trgm",
                table: "ProductListProjections",
                column: "SearchTextUk")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductListProjections");
        }
    }
}
