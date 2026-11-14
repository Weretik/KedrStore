using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogCodeMeanings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogAttributeAliasMeanings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AliasId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    AttributeId = table.Column<int>(type: "integer", nullable: false),
                    ValueId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogAttributeAliasMeanings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeAliasMeanings_CatalogAttributeAliases_Alias~",
                        column: x => x.AliasId,
                        principalTable: "CatalogAttributeAliases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeAliasMeanings_CatalogAttributeValues_ValueId",
                        column: x => x.ValueId,
                        principalTable: "CatalogAttributeValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeAliasMeanings_CatalogAttributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "CatalogAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeAliasMeanings_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeAliasMeanings_AttributeId",
                table: "CatalogAttributeAliasMeanings",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeAliasMeanings_CategoryId",
                table: "CatalogAttributeAliasMeanings",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeAliasMeanings_ValueId",
                table: "CatalogAttributeAliasMeanings",
                column: "ValueId");

            migrationBuilder.CreateIndex(
                name: "UX_CatalogAttributeAliasMeanings_Alias_Category_Attribute",
                table: "CatalogAttributeAliasMeanings",
                columns: new[] { "AliasId", "CategoryId", "AttributeId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogAttributeAliasMeanings");
        }
    }
}
