using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Media.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaFileNameSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_FileName_trgm",
                schema: "media",
                table: "MediaFiles",
                column: "FileName")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_FileName_trgm",
                schema: "media",
                table: "MediaFiles");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
