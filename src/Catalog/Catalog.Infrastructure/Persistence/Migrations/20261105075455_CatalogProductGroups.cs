using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogProductGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogProductGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    GroupKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MainProductId = table.Column<int>(type: "integer", nullable: true),
                    IsMainManual = table.Column<bool>(type: "boolean", nullable: false),
                    IsCreatedManually = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogProductGroups_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogProductGroups_Products_MainProductId",
                        column: x => x.MainProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductGroupMembers",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: true),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductGroupMembers", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_CatalogProductGroupMembers_CatalogProductGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "CatalogProductGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogProductGroupMembers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductGroupMembers_GroupId",
                table: "CatalogProductGroupMembers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductGroups_MainProductId",
                table: "CatalogProductGroups",
                column: "MainProductId");

            migrationBuilder.CreateIndex(
                name: "UX_CatalogProductGroups_Category_Key",
                table: "CatalogProductGroups",
                columns: new[] { "CategoryId", "GroupKey" },
                unique: true,
                filter: "\"GroupKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_CatalogProductGroups_Slug",
                table: "CatalogProductGroups",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogProductGroupMembers");

            migrationBuilder.DropTable(
                name: "CatalogProductGroups");
        }
    }
}
