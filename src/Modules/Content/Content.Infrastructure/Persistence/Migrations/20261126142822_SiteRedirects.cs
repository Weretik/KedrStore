using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SiteRedirects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "SiteRedirects",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ToUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteRedirects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteRedirects_FromPath_trgm",
                schema: "content",
                table: "SiteRedirects",
                column: "FromPath")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteRedirects_ToUrl_trgm",
                schema: "content",
                table: "SiteRedirects",
                column: "ToUrl")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteRedirects_UpdatedAt",
                schema: "content",
                table: "SiteRedirects",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "UX_SiteRedirects_FromPath",
                schema: "content",
                table: "SiteRedirects",
                column: "FromPath",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteRedirects",
                schema: "content");

            // pg_trgm stays: other modules' search indexes use it.
        }
    }
}
