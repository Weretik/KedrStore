using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StableSlugsSoftDeleteCategoryContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "ProductCategories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ProductCategories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CatalogSlugHistory",
                columns: table => new
                {
                    EntityType = table.Column<short>(type: "smallint", nullable: false),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogSlugHistory", x => new { x.EntityType, x.Slug });
                });

            migrationBuilder.CreateTable(
                name: "CategoryContents",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    SeoTitle = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    H1 = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    BodyHtml = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryContents", x => new { x.CategoryId, x.Language });
                    table.ForeignKey(
                        name: "FK_CategoryContents_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryFaqItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Question = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Answer = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryFaqItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryFaqItems_CategoryContents_CategoryId_Language",
                        columns: x => new { x.CategoryId, x.Language },
                        principalTable: "CategoryContents",
                        principalColumns: new[] { "CategoryId", "Language" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Products_ProductSlug",
                table: "Products",
                column: "ProductSlug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ProductCategories_Slug",
                table: "ProductCategories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogSlugHistory_EntityType_EntityId",
                table: "CatalogSlugHistory",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryFaqItems_Content_SortOrder",
                table: "CategoryFaqItems",
                columns: new[] { "CategoryId", "Language", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogSlugHistory");

            migrationBuilder.DropTable(
                name: "CategoryFaqItems");

            migrationBuilder.DropTable(
                name: "CategoryContents");

            migrationBuilder.DropIndex(
                name: "UX_Products_ProductSlug",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "UX_ProductCategories_Slug",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProductCategories");
        }
    }
}
