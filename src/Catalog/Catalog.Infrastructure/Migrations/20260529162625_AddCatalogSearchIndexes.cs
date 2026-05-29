using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:ltree", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_Language_IsDeleted",
                table: "ProductTranslations",
                columns: new[] { "Language", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_Name_trgm",
                table: "ProductTranslations",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ListFilters",
                table: "Products",
                columns: new[] { "CategoryId", "IsDeleted", "IsSale", "IsNew" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name_trgm",
                table: "Products",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductTranslations_Language_IsDeleted",
                table: "ProductTranslations");

            migrationBuilder.DropIndex(
                name: "IX_ProductTranslations_Name_trgm",
                table: "ProductTranslations");

            migrationBuilder.DropIndex(
                name: "IX_Products_ListFilters",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Name_trgm",
                table: "Products");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:ltree", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
