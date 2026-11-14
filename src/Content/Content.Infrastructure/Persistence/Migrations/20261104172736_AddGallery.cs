using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GalleryItems",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    MediaFileId = table.Column<long>(type: "bigint", nullable: true),
                    YoutubeId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    TitleUk = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    TitleRu = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CaptionUk = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CaptionRu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryItems", x => x.Id);
                    table.CheckConstraint("CK_GalleryItems_Source", "(\"Kind\" = 'Photo' AND \"MediaFileId\" IS NOT NULL AND \"YoutubeId\" IS NULL) OR (\"Kind\" = 'Video' AND \"YoutubeId\" IS NOT NULL AND \"MediaFileId\" IS NULL)");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GalleryItems_MediaFileId",
                schema: "content",
                table: "GalleryItems",
                column: "MediaFileId",
                unique: true,
                filter: "\"MediaFileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryItems_Position",
                schema: "content",
                table: "GalleryItems",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryItems_YoutubeId",
                schema: "content",
                table: "GalleryItems",
                column: "YoutubeId",
                unique: true,
                filter: "\"YoutubeId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GalleryItems",
                schema: "content");
        }
    }
}
