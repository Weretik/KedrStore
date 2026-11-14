using Microsoft.EntityFrameworkCore.Migrations;

namespace Content.Infrastructure.Persistence.Seed;

/// <summary>
/// The seven owner-approved videos the site kept in code (newest first, the confirmed order); used once by a data
/// migration, so the gallery looks the same right after the deploy.
/// </summary>
internal static class LegacyGallerySeed
{
    private static readonly DateTimeOffset SeededAt = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    internal static readonly IReadOnlyList<(string YoutubeId, string TitleUk, string TitleRu)> Videos =
    [
        ("0FDRbSaQ9Ec", "Прихована петля ZCG 40", "Петля скрытая ZCG 40"),
        ("j9wrjRub9xI", "Порівняння прихованих петель із ZCG 40 KEDR", "Сравнение скрытых петель с ZCG 40 KEDR"),
        ("1eB0xxFgwlw", "1 квітня 2021 р.", "1 апреля 2021 г."),
        ("8hLIBALylkQ", "CLASS Броненакладка Z 1021 CP (порожниста)", "CLASS Броненакладка Z 1021 CP (пустотелая)"),
        ("LQcVX5dC9MM", "Порівняння замка Apecs 2600 із замком Class 2800", "Сравнение замка Apecs 2600 с замком Class 2800"),
        ("SIWCZ5uVY_4", "CLASS Броненакладка Z 1021 CP (порожниста)", "CLASS Броненакладка Z 1021 CP (пустотелая)"),
        ("ekr7ceJlJcg", "Замок Class 2800 (технічне пакування)", "Замок Class 2800 (тех упаковка)"),
    ];

    public static void Up(MigrationBuilder migrationBuilder)
    {
        for (var position = 0; position < Videos.Count; position++)
        {
            var (youtubeId, titleUk, titleRu) = Videos[position];
            migrationBuilder.InsertData(
                schema: ContentDbContext.Schema, table: "GalleryItems",
                columns: ["Kind", "YoutubeId", "Position", "TitleUk", "TitleRu", "CreatedAt", "UpdatedAt"],
                values: [nameof(GalleryItemKind.Video), youtubeId, position, titleUk, titleRu, SeededAt, SeededAt]);
        }
    }

    public static void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql(
            $"""DELETE FROM content."GalleryItems" WHERE "YoutubeId" IN ({string.Join(", ", Videos.Select(video => $"'{video.YoutubeId}'"))});""");
}
