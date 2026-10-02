using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPhotoChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductPhotoChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    PhotoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CheckedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    DiagnosticCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DiagnosticMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
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

            migrationBuilder.Sql(
                """
                INSERT INTO "ProductPhotoChecks" ("Id", "PhotoUrl", "Status")
                SELECT "Id", "Photo", 'Unknown'
                FROM "Products"
                WHERE NOT "IsDeleted";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPhotoChecks");
        }
    }
}
