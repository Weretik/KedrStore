using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CatalogSiteModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AcceptedAt",
                table: "CatalogProductGroups",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptedBy",
                table: "CatalogProductGroups",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "Status",
                table: "CatalogProductGroups",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            // Groups a person created are accepted models (spec catalog/007 R-01); automatic ones stay proposed.
            migrationBuilder.Sql(
                "UPDATE \"CatalogProductGroups\" SET \"Status\" = 1, \"AcceptedAt\" = \"UpdatedAt\" WHERE \"IsCreatedManually\";");

            migrationBuilder.CreateTable(
                name: "CatalogProductGroupAxes",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    AttributeId = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductGroupAxes", x => new { x.GroupId, x.AttributeId });
                    table.ForeignKey(
                        name: "FK_CatalogProductGroupAxes_CatalogAttributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "CatalogAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogProductGroupAxes_CatalogProductGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "CatalogProductGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductGroupTexts",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    SeoTitle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SeoDescription = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductGroupTexts", x => new { x.GroupId, x.Language });
                    table.ForeignKey(
                        name: "FK_CatalogProductGroupTexts_CatalogProductGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "CatalogProductGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductGroupAxes_AttributeId",
                table: "CatalogProductGroupAxes",
                column: "AttributeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogProductGroupAxes");

            migrationBuilder.DropTable(
                name: "CatalogProductGroupTexts");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "CatalogProductGroups");

            migrationBuilder.DropColumn(
                name: "AcceptedBy",
                table: "CatalogProductGroups");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CatalogProductGroups");
        }
    }
}
