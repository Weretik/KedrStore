using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogAttributesAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutomaticIsConfirmed",
                table: "CatalogProductAttributeValues",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "AutomaticNumberValue",
                table: "CatalogProductAttributeValues",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "AutomaticSource",
                table: "CatalogProductAttributeValues",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AutomaticValueId",
                table: "CatalogProductAttributeValues",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SwatchMediaFileId",
                table: "CatalogAttributeValues",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductAttributeValues_AutomaticValueId",
                table: "CatalogProductAttributeValues",
                column: "AutomaticValueId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogProductAttributeValues_CatalogAttributeValues_Automa~",
                table: "CatalogProductAttributeValues",
                column: "AutomaticValueId",
                principalTable: "CatalogAttributeValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogProductAttributeValues_CatalogAttributeValues_Automa~",
                table: "CatalogProductAttributeValues");

            migrationBuilder.DropIndex(
                name: "IX_CatalogProductAttributeValues_AutomaticValueId",
                table: "CatalogProductAttributeValues");

            migrationBuilder.DropColumn(
                name: "AutomaticIsConfirmed",
                table: "CatalogProductAttributeValues");

            migrationBuilder.DropColumn(
                name: "AutomaticNumberValue",
                table: "CatalogProductAttributeValues");

            migrationBuilder.DropColumn(
                name: "AutomaticSource",
                table: "CatalogProductAttributeValues");

            migrationBuilder.DropColumn(
                name: "AutomaticValueId",
                table: "CatalogProductAttributeValues");

            migrationBuilder.DropColumn(
                name: "SwatchMediaFileId",
                table: "CatalogAttributeValues");
        }
    }
}
