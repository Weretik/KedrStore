using System.Text;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Sync;

/// <summary>Applies Russian product names from the bundled CSV to the products of one import run.</summary>
public sealed class ProductTranslationImporter(
    ICatalogRepository<ProductTranslation> translationRepo,
    ILogger<ProductTranslationImporter> logger)
{
    private const string RuLanguage = "ru";
    private const string TranslationFileRelativePath = "Translation/Imports/product-translations.ru.csv";

    public async Task ImportAsync(IReadOnlyCollection<ProductId> productIds, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
            return;

        var counters = new ImportCounters { Received = productIds.Count };
        var translationsById = ReadTranslationsFromCsv();

        var existingTranslations = await translationRepo.ListAsync(
            new ProductTranslationsByProductIdsAndLanguageSpec(productIds, RuLanguage),
            cancellationToken);

        var existingByProductId = existingTranslations.ToDictionary(x => x.ProductId.Value, x => x);
        var created = new List<ProductTranslation>();
        var now = DateTimeOffset.UtcNow;

        foreach (var productId in productIds)
        {
            var hasTranslation = translationsById.TryGetValue(productId.Value, out var nameRu) &&
                                 !string.IsNullOrWhiteSpace(nameRu);
            existingByProductId.TryGetValue(productId.Value, out var existing);

            if (!hasTranslation)
            {
                if (existing is { IsDeleted: false })
                {
                    existing.MarkAsDeleted(now);
                    counters.Deleted++;
                }
                else
                {
                    counters.Unchanged++;
                }

                continue;
            }

            if (existing is null)
            {
                created.Add(ProductTranslation.Create(productId, RuLanguage, nameRu!, now));
                counters.Created++;
            }
            else if (existing.Apply(nameRu!, now))
            {
                counters.Updated++;
            }
            else
            {
                counters.Unchanged++;
            }
        }

        if (created.Count > 0)
            await translationRepo.AddRangeAsync(created, cancellationToken);

        await translationRepo.SaveChangesAsync(cancellationToken);
        counters.Log(logger, "Product translations", RuLanguage);
    }

    private Dictionary<int, string> ReadTranslationsFromCsv()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, TranslationFileRelativePath);
        if (!File.Exists(filePath))
        {
            logger.LogWarning(
                "Product translations file was not found. Expected path: {FilePath}. Translations sync will be skipped for this run.",
                filePath);
            return new Dictionary<int, string>();
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        _ = reader.ReadLine();

        var result = new Dictionary<int, string>();
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = ParseCsvLine(line);
            if (fields.Count < 3)
                continue;

            if (!int.TryParse(fields[0].Trim(), out var productId) || productId <= 0)
                continue;

            var ruName = fields[2].Trim();
            if (string.IsNullOrWhiteSpace(ruName))
                continue;

            result[productId] = ruName;
        }

        return result;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                fields.Add(sb.ToString());
                sb.Clear();
                continue;
            }

            sb.Append(ch);
        }

        fields.Add(sb.ToString());
        return fields;
    }
}
