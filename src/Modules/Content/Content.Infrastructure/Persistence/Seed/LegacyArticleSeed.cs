using Microsoft.EntityFrameworkCore.Migrations;

namespace Content.Infrastructure.Persistence.Seed;

/// <summary>Inserts <see cref="LegacyArticles"/> with their covers; used once by a data migration.</summary>
internal static class LegacyArticleSeed
{
    private static readonly DateTimeOffset SeededAt = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    public static void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var article in LegacyArticles.All)
        {
            migrationBuilder.InsertData(
                schema: ContentDbContext.Schema, table: "Articles",
                columns: ["Id", "Slug", "PublishedOn", "IsPublished", "CoverVersion", "CreatedAt", "UpdatedAt"],
                values: [article.Id, article.Slug, article.PublishedOn, true, 1, SeededAt, SeededAt]);
            foreach (var text in article.Texts)
            {
                migrationBuilder.InsertData(
                    schema: ContentDbContext.Schema, table: "ArticleTranslations",
                    columns: ["ArticleId", "Language", "Title", "Excerpt", "BodyHtml"],
                    values: [article.Id, text.Language, text.Title, text.Excerpt, text.BodyHtml]);
            }

            migrationBuilder.InsertData(
                schema: ContentDbContext.Schema, table: "ArticleCovers",
                columns: ["ArticleId", "Data", "ContentType"],
                values: [article.Id, ReadCover(article.Id), "image/webp"]);
        }

        // Explicit ids do not move the identity sequence; new articles must start after the seeded ones.
        migrationBuilder.Sql(
            """SELECT setval(pg_get_serial_sequence('content."Articles"', 'Id'), (SELECT MAX("Id") FROM content."Articles"));""");
    }

    public static void Down(MigrationBuilder migrationBuilder)
    {
        var ids = LegacyArticles.All.Select(article => article.Id).ToArray();
        migrationBuilder.Sql($"""DELETE FROM content."Articles" WHERE "Id" IN ({string.Join(", ", ids)});""");
    }

    private static byte[] ReadCover(long id)
    {
        var name = $"Content.Infrastructure.Seed.Covers.article-{id}.webp";
        using var stream = typeof(LegacyArticleSeed).Assembly.GetManifestResourceStream(name)
                           ?? throw new InvalidOperationException($"Missing embedded cover {name}.");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
