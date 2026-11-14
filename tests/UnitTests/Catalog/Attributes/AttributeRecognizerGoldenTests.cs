using System.Globalization;
using System.Text.Json.Nodes;
using Catalog.Application.Features.Attributes.Shared.Import;
using Catalog.Application.Features.Attributes.Shared.Recognition;

namespace UnitTests.Catalog.Attributes;

/// <summary>
/// Catalog attributes 006, phase 4: the .NET recognizer gives exactly the committed
/// <c>docs/catalog-attributes/products_attributes.csv</c> for every product (2330/2330), from the original name,
/// the category, and <c>dictionaries.json</c>.
/// </summary>
public sealed class AttributeRecognizerGoldenTests
{
    private const string NullLiteral = "null";

    [Fact]
    public void EveryProduct_MatchesTheCsv()
    {
        var rows = ReadCsv();
        var paths = rows.Where(row => row["category_path"] != NullLiteral)
            .GroupBy(row => int.Parse(row["category_id"], CultureInfo.InvariantCulture))
            .ToDictionary(group => group.Key, group => group.First()["category_path"]);
        var categories = CategoryPaths.FromPaths(paths);
        using var file = File.OpenRead(RepositoryFile("docs/catalog-attributes/dictionaries.json"));
        var dictionary = RecognitionDictionary.Read(file, categories);

        var mismatches = new List<string>();
        foreach (var expected in rows)
        {
            var product = new RecognitionProduct(
                int.Parse(expected["product_id"], CultureInfo.InvariantCulture),
                expected["category_id"] == NullLiteral ? null : int.Parse(expected["category_id"], CultureInfo.InvariantCulture),
                expected["original_name"]);
            var actual = ProductAttributeRecognizer.Recognize(product, dictionary, categories);
            foreach (var column in RecognitionRow.Columns)
            {
                var cell = actual.Cell(column) ?? NullLiteral;
                var same = column == "extra_attributes"
                    ? JsonNode.DeepEquals(JsonNode.Parse(expected[column]), JsonNode.Parse(cell))
                    : expected[column] == cell;
                if (!same)
                {
                    mismatches.Add($"{product.Id} {column}: expected «{Short(expected[column])}», got «{Short(cell)}»");
                    break;
                }
            }
        }

        Assert.Equal(2330, rows.Count);
        Assert.True(mismatches.Count == 0,
            $"{mismatches.Count} of {rows.Count} products differ:\n" + string.Join("\n", mismatches.Take(40)));
    }

    private static string Short(string text) => text.Length > 400 ? text[..400] + "…" : text;

    private static List<Dictionary<string, string>> ReadCsv()
    {
        using var reader = new StreamReader(RepositoryFile("docs/catalog-attributes/products_attributes.csv"));
        var records = CsvText.ReadRecords(reader).ToList();
        var header = records[0].Fields.Select(name => name.TrimStart('\uFEFF')).ToList();
        return records.Skip(1)
            .Where(record => record.Fields.Count == header.Count)
            .Select(record => header.Select((name, index) => (name, value: record.Fields[index]))
                .ToDictionary(pair => pair.name, pair => pair.value))
            .ToList();
    }

    private static string RepositoryFile(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException($"Cannot find {relativePath} above {AppContext.BaseDirectory}.");
    }
}
