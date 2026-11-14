using System.Text.Json.Nodes;
using static Catalog.Application.Features.Attributes.Shared.Recognition.RecognitionPatterns;

namespace Catalog.Application.Features.Attributes.Shared.Recognition;

/// <summary>
/// Recognizes one product name into a <see cref="RecognitionRow"/> (Python <c>Extraction</c>): attribute values,
/// colors, sizes, packaging, the base name left after removing them, and the reasons a person must review it.
/// </summary>
internal sealed partial class ProductNameRecognition
{
    private static readonly HashSet<string> RemovedAttributes =
    [
        "color", "color_combination", "color_alias_expression", "side", "size_variant", "branding", "packaging",
        "bearing_designation", "door_infill"
    ];

    private readonly RecognitionProduct _product;
    private readonly RecognitionDictionary _dictionary;
    private readonly CategoryPaths _categories;
    private readonly string _name;
    private readonly string _kind;
    private readonly Dictionary<string, List<string>> _reasons = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<string>> _information = new(StringComparer.Ordinal);
    private readonly HashSet<string> _conflictedFields = new(StringComparer.Ordinal);
    private readonly List<(int Start, int End)> _remove = [];
    private readonly JsonObject _extra;
    private readonly RecognitionRow _row;
    private List<NameObservation> _observations = [];

    public ProductNameRecognition(RecognitionProduct product, RecognitionDictionary dictionary, CategoryPaths categories)
    {
        _product = product;
        _dictionary = dictionary;
        _categories = categories;
        _name = product.Name;
        _kind = ProductKinds.Of(product, dictionary);
        _extra = new JsonObject { ["product_kind"] = _kind };
        _row = new RecognitionRow
        {
            ProductId = product.Id,
            CategoryId = product.CategoryId,
            CategoryPath = product.CategoryId is { } id ? categories.Paths.GetValueOrDefault(id) : null,
            OriginalName = product.Name
        };
    }

    private void Issue(string code, string text)
    {
        if (!_reasons.TryGetValue(code, out var texts))
            _reasons[code] = texts = [];
        if (!texts.Contains(text))
            texts.Add(text);
    }

    private void Inform(string code, string text)
    {
        if (!_information.TryGetValue(code, out var texts))
            _information[code] = texts = [];
        if (!texts.Contains(text))
            texts.Add(text);
    }

    private void Drop(int start, int end)
    {
        if (0 <= start && start < end && end <= _name.Length)
            _remove.Add((start, end));
    }

    /// <summary>Sets a size number; two different values make it empty with a review reason.</summary>
    private void Field(string key, decimal? value)
    {
        if (_conflictedFields.Contains(key))
            return;

        var current = key switch
        {
            "size_length" => _row.SizeLength,
            "size_width" => _row.SizeWidth,
            "size_thickness" => _row.SizeThickness,
            "size_diameter" => _row.SizeDiameter,
            _ => throw new ArgumentOutOfRangeException(nameof(key))
        };
        if (current is not null && current != value)
        {
            SetField(key, null);
            _conflictedFields.Add(key);
            Issue("conflicting_" + key, $"Несколько разных значений для {key}; итог оставлен null.");
        }
        else
        {
            SetField(key, value);
        }
    }

    private void SetField(string key, decimal? value)
    {
        switch (key)
        {
            case "size_length": _row.SizeLength = value; break;
            case "size_width": _row.SizeWidth = value; break;
            case "size_thickness": _row.SizeThickness = value; break;
            case "size_diameter": _row.SizeDiameter = value; break;
        }
    }

    /// <summary>Runs every stage; Python <c>finish</c>.</summary>
    public RecognitionRow Finish()
    {
        if (string.IsNullOrWhiteSpace(_name))
        {
            Issue("invalid_name", "Название отсутствует или не является непустой строкой.");
        }
        else
        {
            Lexical();
            Colors();
            Measurements();
            Packaging();
            BaseName();
            if (_kind == "service")
                Inform("service_record", "Услуга: физические размер и цвет могут быть неприменимы.");
            if (_kind == "advertising")
                Inform("advertising_record", "Рекламная/выставочная позиция.");
            if (_product.CategoryId is { } mismatch && _dictionary.KnownCategoryMismatchIds.Contains(mismatch))
                Inform("category_name_mismatch", "Название исходной категории не соответствует виду изделия; category_id и путь сохранены.");
        }

        if (_product.CategoryId is not { } categoryId || !_categories.Paths.ContainsKey(categoryId))
            Issue("category_not_found", "Категория товара отсутствует в файле категорий.");
        if (_product.CategoryId is { } known && _categories.Issues.TryGetValue(known, out var issues))
        {
            foreach (var code in issues)
                Issue(code, "Неполный или циклический путь категории.");
        }

        if (_row.BaseName is null && !string.IsNullOrWhiteSpace(_name))
            Issue("base_name_empty_after_extraction", "После извлечения атрибутов не осталось названия модели.");

        var reasonCodes = _reasons.Keys.Order(StringComparer.Ordinal).ToList();
        _extra["review_reason_codes"] = new JsonArray(reasonCodes.Select(code => (JsonNode?)code).ToArray());
        if (_information.Count > 0)
            _extra["information_codes"] = new JsonArray(_information.Keys.Order(StringComparer.Ordinal).Select(code => (JsonNode?)code).ToArray());
        _row.ExtraAttributes = _extra;
        _row.NeedsReview = _reasons.Count > 0;

        var ordered = _reasons.Keys.OrderBy(Priority).ThenBy(code => code, StringComparer.Ordinal).ToList();
        _extra["primary_review_reason_code"] = ordered.Count > 0 ? ordered[0] : null;
        _row.ReviewReason = ordered.Count > 0 ? _reasons[ordered[0]][0] : null;
        _row.Notes = string.Join(" ", ordered.SelectMany(code => _reasons[code])
            .Concat(_information.Keys.Order(StringComparer.Ordinal).SelectMany(code => _information[code])));
        return _row;
    }

    private static int Priority(string code)
    {
        if (code.StartsWith("conflicting_", StringComparison.Ordinal) || code is "brand_marking_conflict" or "ambiguous_side" or "multiple_unassigned_colors")
            return 0;
        if (code.StartsWith("invalid_", StringComparison.Ordinal) || code == "multiple_size_expressions")
            return 1;
        if (code is "unresolved_color" or "unresolved_color_combination" or "unresolved_color_alias_expression")
            return 2;
        return 3;
    }

    private static JsonArray Strings(IEnumerable<string>? values)
        => new((values ?? []).Select(value => (JsonNode?)value).ToArray());

    private static JsonObject Component(ColorComponent component) => new()
    {
        ["raw_value"] = component.RawValue,
        ["normalized_code"] = component.NormalizedCode,
        ["normalized_value_uk"] = component.NormalizedValueUk,
        ["status"] = component.Status
    };

    private static bool Overlaps(int start, int end, IEnumerable<(int, int)> spans) => Overlap(start, end, spans);
}
