using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductPrices_ProductId_PriceTypeId",
                table: "ProductPrices");

            migrationBuilder.AddColumn<string>(
                name: "ProductSlug",
                table: "Products",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductTypeIdOneC",
                table: "Products",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductPrices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "ProductTypeIdOneC",
                table: "ProductPrices",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductTypeIdOneC",
                table: "ProductCategories",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PriceType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    PriceTypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductId_PriceTypeId",
                table: "ProductPrices",
                columns: new[] { "ProductId", "PriceTypeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceType");

            migrationBuilder.DropIndex(
                name: "IX_ProductPrices_ProductId_PriceTypeId",
                table: "ProductPrices");

            migrationBuilder.DropColumn(
                name: "ProductSlug",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductTypeIdOneC",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductTypeIdOneC",
                table: "ProductPrices");

            migrationBuilder.DropColumn(
                name: "ProductTypeIdOneC",
                table: "ProductCategories");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductPrices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductId_PriceTypeId",
                table: "ProductPrices",
                columns: new[] { "ProductId", "PriceTypeId" });
        }
    }
}
