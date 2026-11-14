using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "Articles",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PublishedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CoverVersion = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleCovers",
                schema: "content",
                columns: table => new
                {
                    ArticleId = table.Column<long>(type: "bigint", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleCovers", x => x.ArticleId);
                    table.ForeignKey(
                        name: "FK_ArticleCovers_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalSchema: "content",
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTranslations",
                schema: "content",
                columns: table => new
                {
                    Language = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    ArticleId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Excerpt = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    BodyHtml = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTranslations", x => new { x.ArticleId, x.Language });
                    table.ForeignKey(
                        name: "FK_ArticleTranslations_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalSchema: "content",
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IsPublished_PublishedOn",
                schema: "content",
                table: "Articles",
                columns: new[] { "IsPublished", "PublishedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Slug",
                schema: "content",
                table: "Articles",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleCovers",
                schema: "content");

            migrationBuilder.DropTable(
                name: "ArticleTranslations",
                schema: "content");

            migrationBuilder.DropTable(
                name: "Articles",
                schema: "content");
        }
    }
}
