using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductMediaLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPhotoChecks");

            migrationBuilder.AlterColumn<string>(
                name: "Sсheme",
                table: "Products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "Products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.CreateTable(
                name: "ProductMedia",
                columns: table => new
                {
                    MediaFileId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMedia", x => new { x.ProductId, x.MediaFileId });
                    table.ForeignKey(
                        name: "FK_ProductMedia_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductMedia_MediaFileId",
                table: "ProductMedia",
                column: "MediaFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductMedia");

            migrationBuilder.AlterColumn<string>(
                name: "Sсheme",
                table: "Products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "Products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldDefaultValue: "");

            migrationBuilder.CreateTable(
                name: "ProductPhotoChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    CheckedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DiagnosticCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DiagnosticMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPhotoChecks", x => x.Id);
                    table.CheckConstraint("CK_ProductPhotoChecks_State", "(\"Status\" = 'Unknown' AND \"CheckedAtUtc\" IS NULL AND \"HttpStatusCode\" IS NULL AND \"DiagnosticCode\" IS NULL AND \"DiagnosticMessage\" IS NULL)\nOR (\"Status\" = 'Available' AND \"CheckedAtUtc\" IS NOT NULL AND \"HttpStatusCode\" BETWEEN 200 AND 299 AND \"DiagnosticCode\" IS NULL AND \"DiagnosticMessage\" IS NULL)\nOR (\"Status\" = 'Missing' AND \"CheckedAtUtc\" IS NOT NULL AND \"HttpStatusCode\" IN (404, 410) AND \"DiagnosticCode\" IS NOT NULL)\nOR (\"Status\" = 'InvalidContentType' AND \"CheckedAtUtc\" IS NOT NULL AND \"HttpStatusCode\" BETWEEN 200 AND 299 AND \"DiagnosticCode\" IS NOT NULL)\nOR (\"Status\" = 'CheckFailed' AND \"CheckedAtUtc\" IS NOT NULL AND (\"HttpStatusCode\" IS NULL OR \"HttpStatusCode\" BETWEEN 100 AND 599) AND \"DiagnosticCode\" IS NOT NULL)");
                    table.CheckConstraint("CK_ProductPhotoChecks_Status", "\"Status\" IN ('Unknown', 'Available', 'Missing', 'InvalidContentType', 'CheckFailed')");
                    table.ForeignKey(
                        name: "FK_ProductPhotoChecks_Products_Id",
                        column: x => x.Id,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotoChecks_Status_ProductId",
                table: "ProductPhotoChecks",
                columns: new[] { "Status", "Id" });
        }
    }
}
