using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLegalDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LegalDocumentVersions",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocumentVersions", x => x.Id);
                    table.CheckConstraint("CK_LegalDocumentVersions_Published", "\"Status\" = 'Draft' OR (\"Number\" IS NOT NULL AND \"EffectiveFrom\" IS NOT NULL AND \"PublishedAt\" IS NOT NULL)");
                });

            migrationBuilder.CreateTable(
                name: "LegalDocumentTexts",
                schema: "content",
                columns: table => new
                {
                    Language = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    VersionId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BodyHtml = table.Column<string>(type: "character varying(100000)", maxLength: 100000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocumentTexts", x => new { x.VersionId, x.Language });
                    table.ForeignKey(
                        name: "FK_LegalDocumentTexts_LegalDocumentVersions_VersionId",
                        column: x => x.VersionId,
                        principalSchema: "content",
                        principalTable: "LegalDocumentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocumentVersions_Kind_Draft",
                schema: "content",
                table: "LegalDocumentVersions",
                column: "Kind",
                unique: true,
                filter: "\"Status\" = 'Draft'");

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocumentVersions_Kind_Number",
                schema: "content",
                table: "LegalDocumentVersions",
                columns: new[] { "Kind", "Number" },
                unique: true,
                filter: "\"Number\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocumentVersions_Kind_Published",
                schema: "content",
                table: "LegalDocumentVersions",
                column: "Kind",
                unique: true,
                filter: "\"Status\" = 'Published'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LegalDocumentTexts",
                schema: "content");

            migrationBuilder.DropTable(
                name: "LegalDocumentVersions",
                schema: "content");
        }
    }
}
