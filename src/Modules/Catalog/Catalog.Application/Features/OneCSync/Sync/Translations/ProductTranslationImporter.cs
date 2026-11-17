using System.Text;
using Catalog.Application.Abstractions.Persistence;
namespace Catalog.Application.Features.OneCSync.Sync.Translations;

/// <summary>Applies Russian product names from the bundled CSV to the products of one import run.</summary>
public sealed class ProductTranslationImporter(
    ICatalogRepository<Product> productRepo,
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
        var products = await productRepo.ListAsync(
            new ProductsWithTranslationByIdsSpec(productIds, RuLanguage), cancellationToken);
        var now = DateTimeOffset.UtcNow;

        foreach (var product in products)
        {
            if (!translationsById.TryGetValue(product.Id.Value, out var nameRu) || string.IsNullOrWhiteSpace(nameRu))
            {
                if (product.RemoveTranslation(RuLanguage, now))
                    counters.Deleted++;
                else
                    counters.Unchanged++;
                continue;
            }

            var changed = product.SetTranslation(RuLanguage, nameRu, now);
            if (changed.IsFailure)
                counters.Skipped++;
            else if (changed.Value == TranslationChange.Created)
                counters.Created++;
            else if (changed.Value == TranslationChange.Updated)
                counters.Updated++;
            else
                counters.Unchanged++;
        }

        await productRepo.SaveChangesAsync(cancellationToken);
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
