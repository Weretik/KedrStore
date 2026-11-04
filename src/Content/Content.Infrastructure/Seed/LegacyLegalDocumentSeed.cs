using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Content.Infrastructure.Seed;

/// <summary>
/// Inserts the four policy texts the site kept in code as version 1, published from 2026-11-03
/// (the date the owner confirmed them); used once by a data migration.
/// </summary>
internal static class LegacyLegalDocumentSeed
{
    private const string ResourceName = "Content.Infrastructure.Seed.LegalDocuments.legal-documents.json";
    private const string SeededBy = "seed";
    private static readonly DateOnly EffectiveFrom = new(2026, 11, 3);
    private static readonly DateTimeOffset SeededAt = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    public static void Up(MigrationBuilder migrationBuilder)
    {
        var id = 0L;
        foreach (var document in Read())
        {
            id++;
            migrationBuilder.InsertData(
                schema: ContentDbContext.Schema, table: "LegalDocumentVersions",
                columns: ["Id", "Kind", "Status", "Number", "EffectiveFrom", "UpdatedAt", "UpdatedBy", "PublishedAt", "PublishedBy"],
                values: [id, document.Kind, nameof(LegalDocumentStatus.Published), 1, EffectiveFrom, SeededAt, SeededBy, SeededAt, SeededBy]);
            foreach (var text in document.Texts)
            {
                migrationBuilder.InsertData(
                    schema: ContentDbContext.Schema, table: "LegalDocumentTexts",
                    columns: ["VersionId", "Language", "Title", "BodyHtml"],
                    values: [id, text.Language, text.Title, text.BodyHtml]);
            }
        }

        // Explicit ids do not move the identity sequence; new versions must start after the seeded ones.
        migrationBuilder.Sql(
            """SELECT setval(pg_get_serial_sequence('content."LegalDocumentVersions"', 'Id'), (SELECT MAX("Id") FROM content."LegalDocumentVersions"));""");
    }

    public static void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("""DELETE FROM content."LegalDocumentVersions" WHERE "UpdatedBy" = 'seed' AND "Number" = 1;""");

    internal static IReadOnlyList<SeedDocument> Read()
    {
        using var stream = typeof(LegacyLegalDocumentSeed).Assembly.GetManifestResourceStream(ResourceName)
                           ?? throw new InvalidOperationException($"Missing embedded seed {ResourceName}.");
        return JsonSerializer.Deserialize<List<SeedDocument>>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
               ?? throw new InvalidOperationException($"Empty seed {ResourceName}.");
    }

    internal sealed record SeedDocument(string Kind, IReadOnlyList<SeedText> Texts);

    internal sealed record SeedText(string Language, string Title, string BodyHtml);
}
