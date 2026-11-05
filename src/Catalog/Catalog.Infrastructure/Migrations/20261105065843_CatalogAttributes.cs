using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CatalogAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NameUk = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NameRu = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Kind = table.Column<short>(type: "smallint", nullable: false),
                    Unit = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsFilterable = table.Column<bool>(type: "boolean", nullable: false),
                    IsVariantAxis = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEditedManually = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogAttributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductAttributeReviews",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    BaseName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    State = table.Column<short>(type: "smallint", nullable: false),
                    Reasons = table.Column<string>(type: "jsonb", nullable: false),
                    AcceptedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductAttributeReviews", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_CatalogProductAttributeReviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogAttributeValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttributeId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NameUk = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    NameRu = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    SwatchHex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    IsEditedManually = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogAttributeValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeValues_CatalogAttributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "CatalogAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogCategoryAttributes",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    AttributeId = table.Column<int>(type: "integer", nullable: false),
                    IsFilterable = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEditedManually = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogCategoryAttributes", x => new { x.CategoryId, x.AttributeId });
                    table.ForeignKey(
                        name: "FK_CatalogCategoryAttributes_CatalogAttributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "CatalogAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogCategoryAttributes_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogAttributeAliases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttributeId = table.Column<int>(type: "integer", nullable: false),
                    Scope = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RawValue = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ValueId = table.Column<int>(type: "integer", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsEditedManually = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogAttributeAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeAliases_CatalogAttributeValues_ValueId",
                        column: x => x.ValueId,
                        principalTable: "CatalogAttributeValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeAliases_CatalogAttributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "CatalogAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductAttributeValues",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    AttributeId = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<short>(type: "smallint", nullable: false),
                    ValueId = table.Column<int>(type: "integer", nullable: true),
                    NumberValue = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    Source = table.Column<short>(type: "smallint", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductAttributeValues", x => new { x.ProductId, x.AttributeId, x.Position });
                    table.ForeignKey(
                        name: "FK_CatalogProductAttributeValues_CatalogAttributeValues_ValueId",
                        column: x => x.ValueId,
                        principalTable: "CatalogAttributeValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogProductAttributeValues_CatalogAttributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "CatalogAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogProductAttributeValues_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeAliases_ValueId",
                table: "CatalogAttributeAliases",
                column: "ValueId");

            migrationBuilder.CreateIndex(
                name: "UX_CatalogAttributeAliases_Attribute_Scope_Raw",
                table: "CatalogAttributeAliases",
                columns: new[] { "AttributeId", "Scope", "RawValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CatalogAttributes_Code",
                table: "CatalogAttributes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CatalogAttributes_Slug",
                table: "CatalogAttributes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CatalogAttributeValues_Attribute_Code",
                table: "CatalogAttributeValues",
                columns: new[] { "AttributeId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CatalogAttributeValues_Attribute_Slug",
                table: "CatalogAttributeValues",
                columns: new[] { "AttributeId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogCategoryAttributes_AttributeId",
                table: "CatalogCategoryAttributes",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductAttributeReviews_BaseName",
                table: "CatalogProductAttributeReviews",
                column: "BaseName");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductAttributeReviews_State",
                table: "CatalogProductAttributeReviews",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductAttributeValues_Attribute_Value",
                table: "CatalogProductAttributeValues",
                columns: new[] { "AttributeId", "ValueId" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductAttributeValues_ValueId",
                table: "CatalogProductAttributeValues",
                column: "ValueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogAttributeAliases");

            migrationBuilder.DropTable(
                name: "CatalogCategoryAttributes");

            migrationBuilder.DropTable(
                name: "CatalogProductAttributeReviews");

            migrationBuilder.DropTable(
                name: "CatalogProductAttributeValues");

            migrationBuilder.DropTable(
                name: "CatalogAttributeValues");

            migrationBuilder.DropTable(
                name: "CatalogAttributes");
        }
    }
}
