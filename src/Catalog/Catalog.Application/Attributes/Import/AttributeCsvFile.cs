using System.Text.Json;

namespace Catalog.Application.Attributes.Import;

/// <summary>
/// Reads the recognizer output (<c>docs/catalog-attributes/products_attributes.csv</c>). The literal <c>null</c>
/// means an empty cell. Original names are read for nothing but error messages; they are never stored.
/// </summary>
public static class AttributeCsvFile
{
    private const string NullLiteral = "null";
    private const string Assistant = "assistant";

    private static readonly string[] RequiredColumns =
    [
        "product_id", "base_name", "size", "size_length", "size_width", "size_thickness", "size_diameter",
        "color_code", "color_secondary_code", "side", "extra_attributes", "needs_review", "review_reason"
    ];

    public sealed record Result(
        IReadOnlyList<RecognizedProduct> Products,
        IReadOnlyList<AttributeValueDefinition> SizeValues,
        IReadOnlyList<string> Errors);

    public static Result Read(TextReader reader)
    {
        var products = new List<RecognizedProduct>();
        var sizes = new Dictionary<string, string>(StringComparer.Ordinal);
        var errors = new List<string>();
        Dictionary<string, int>? columns = null;

        foreach (var (lineNumber, fields) in CsvText.ReadRecords(reader))
        {
            if (columns is null)
            {
                columns = fields.Select((name, index) => (Name: name.Trim().TrimStart('﻿'), index))
                    .ToDictionary(column => column.Name, column => column.index, StringComparer.Ordinal);
                var missing = RequiredColumns.Where(name => !columns.ContainsKey(name)).ToList();
                if (missing.Count > 0)
                    return new Result([], [], [$"The CSV has no columns: {string.Join(", ", missing)}."]);

                continue;
            }

            if (fields.Count == 1 && string.IsNullOrWhiteSpace(fields[0]))
                continue;

            try
            {
                products.Add(FromCells(lineNumber, name => Cell(fields, columns, name), sizes));
            }
            catch (Exception exception) when (exception is FormatException or System.Text.Json.JsonException or KeyNotFoundException
                                                  or InvalidOperationException)
            {
                errors.Add($"Line {lineNumber}: {exception.Message}");
            }
        }

        return new Result(products, ToSizeValues(sizes), errors);
    }

    /// <summary>Size values found in the rows (code → text), ordered by their numbers.</summary>
    public static IReadOnlyList<AttributeValueDefinition> ToSizeValues(IReadOnlyDictionary<string, string> sizes)
        => sizes
            .OrderBy(size => SizeSortKey(size.Value), StringComparer.Ordinal)
            .Select((size, index) => new AttributeValueDefinition(AttributeImportCatalog.Size, size.Key, size.Value, index))
            .ToList();

    /// <summary>Stable code of a size expression: «100×75×2.5 mm» → <c>100x75x2.5mm</c>.</summary>
    public static string SizeCode(string sizeText)
        => new string(sizeText.Trim().ToLowerInvariant().Replace('×', 'x').Replace('х', 'x')
            .Where(character => !char.IsWhiteSpace(character)).ToArray());

    /// <summary>
    /// One recognizer row (from the CSV or the .NET recognizer) as product values and review reasons. A cell is
    /// null when empty. New size texts are added to <paramref name="sizes"/>.
    /// </summary>
    public static RecognizedProduct FromCells(int lineNumber, Func<string, string?> cell, Dictionary<string, string> sizes)
    {
        var productId = int.Parse(cell("product_id") ?? throw new FormatException("product_id is empty."),
            NumberStyles.Integer, CultureInfo.InvariantCulture);
        var baseName = cell("base_name") ?? throw new FormatException("base_name is empty.");
        var extra = JsonDocument.Parse(cell("extra_attributes") ?? "{}").RootElement;
        var values = new List<RecognizedValue>();

        var assistantColors = ColorCodesPickedOnlyByAssistant(extra);
        AddColor(values, cell("color_code"), 0, assistantColors);
        AddColor(values, cell("color_secondary_code"), 1, assistantColors);

        if (cell("size") is { } size)
        {
            var code = SizeCode(size);
            sizes.TryAdd(code, size.Trim());
            values.Add(new RecognizedValue(AttributeImportCatalog.Size, 0, code, null, true));
        }

        foreach (var attribute in AttributeImportCatalog.SizeNumberAttributes)
        {
            if (cell(attribute) is { } number)
            {
                values.Add(new RecognizedValue(attribute, 0, null,
                    decimal.Parse(number, NumberStyles.Number, CultureInfo.InvariantCulture), true));
            }
        }

        if (cell("side") is { } side)
            values.Add(new RecognizedValue(AttributeImportCatalog.Side, 0, side, null, true));

        if (extra.TryGetProperty("attributes", out var attributes) && attributes.ValueKind == JsonValueKind.Object)
        {
            foreach (var attribute in attributes.EnumerateObject())
            {
                var position = 0;
                foreach (var item in attribute.Value.EnumerateArray())
                {
                    if (Text(item, "code") is { } code)
                        values.Add(new RecognizedValue(attribute.Name, position++, code, null, true));
                }
            }
        }

        return new RecognizedProduct(productId, lineNumber, baseName, values,
            Reasons(extra, cell("review_reason"), values));
    }

    private static void AddColor(List<RecognizedValue> values, string? code, int position,
        IReadOnlySet<string> assistantColors)
    {
        if (code is not null)
            values.Add(new RecognizedValue(AttributeImportCatalog.Color, position, code, null,
                !assistantColors.Contains(code)));
    }

    /// <summary>Color codes in the name that only an assistant decoded and the owner has not confirmed.</summary>
    private static HashSet<string> ColorCodesPickedOnlyByAssistant(JsonElement extra)
    {
        if (!extra.TryGetProperty("color_mentions", out var mentions) || mentions.ValueKind != JsonValueKind.Array)
            return [];

        return mentions.EnumerateArray()
            .Where(mention => Text(mention, "code") is not null)
            .GroupBy(mention => Text(mention, "code")!, StringComparer.Ordinal)
            .Where(group => group.All(mention => Text(mention, "selected_by") == Assistant))
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static List<RecognizedReason> Reasons(JsonElement extra, string? reasonText,
        IReadOnlyList<RecognizedValue> values)
    {
        var primary = Text(extra, "primary_review_reason_code");
        var reasons = new List<RecognizedReason>();
        if (extra.TryGetProperty("review_reason_codes", out var codes) && codes.ValueKind == JsonValueKind.Array)
        {
            foreach (var code in codes.EnumerateArray().Select(code => code.GetString()).OfType<string>())
                reasons.Add(new RecognizedReason(code, code == primary ? reasonText : null));
        }

        // Owner decision 2026-11-05: every product without brand marking is reviewed by a person.
        if (values.Any(value => value.AttributeCode == AttributeImportCatalog.Branding
                                && value.ValueCode == AttributeImportCatalog.NoNameCode))
        {
            reasons.Add(new RecognizedReason(Domain.Attributes.AttributeReviewReason.NoName, null));
        }

        return reasons;
    }

    private static string? Cell(IReadOnlyList<string> fields, Dictionary<string, int> columns, string name)
    {
        var index = columns[name];
        var value = index < fields.Count ? fields[index].Trim() : string.Empty;
        return value.Length == 0 || value == NullLiteral ? null : value;
    }

    private static string? Text(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    /// <summary>Orders sizes by their numbers, so «75×62» comes before «100×62».</summary>
    private static string SizeSortKey(string size)
    {
        var numbers = System.Text.RegularExpressions.Regex.Matches(size, @"\d+(?:[.,]\d+)?")
            .Select(match => decimal.Parse(match.Value.Replace(',', '.'), CultureInfo.InvariantCulture));
        return string.Join("|", numbers.Select(number => number.ToString("000000.000", CultureInfo.InvariantCulture)))
               + "|" + size;
    }
}
