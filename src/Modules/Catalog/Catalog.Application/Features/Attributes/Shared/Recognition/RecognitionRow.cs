using System.Text.Json;
using System.Text.Json.Nodes;
using Catalog.Application.Features.Attributes.Shared.Recognition.Dictionary;

namespace Catalog.Application.Features.Attributes.Shared.Recognition;

/// <summary>
/// The recognition result of one product in the columns of <c>products_attributes.csv</c>. The original name is
/// carried unchanged for comparison and messages only.
/// </summary>
public sealed class RecognitionRow
{
    public static readonly IReadOnlyList<string> Columns =
    [
        "product_id", "category_id", "category_path", "original_name", "base_name", "size", "size_length", "size_width",
        "size_thickness", "size_diameter", "color_raw", "color_code", "color_name", "color_secondary_code",
        "color_secondary_name", "side", "extra_attributes", "needs_review", "review_reason", "notes"
    ];

    public required int ProductId { get; init; }
    public int? CategoryId { get; init; }
    public string? CategoryPath { get; init; }
    public required string OriginalName { get; init; }
    public string? BaseName { get; set; }
    public string? Size { get; set; }
    public decimal? SizeLength { get; set; }
    public decimal? SizeWidth { get; set; }
    public decimal? SizeThickness { get; set; }
    public decimal? SizeDiameter { get; set; }
    public string? ColorRaw { get; set; }
    public string? ColorCode { get; set; }
    public string? ColorName { get; set; }
    public string? ColorSecondaryCode { get; set; }
    public string? ColorSecondaryName { get; set; }
    public string? Side { get; set; }
    public JsonObject ExtraAttributes { get; set; } = new();
    public bool NeedsReview { get; set; }
    public string? ReviewReason { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Known spellings found in the name, decoded or not: (attribute, scope, folded text). The owner's extra meanings
    /// of a code (spec catalog/009) reach exactly these products. Not a CSV column.
    /// </summary>
    public HashSet<(string Attribute, string Scope, string Code)> FoundCodes { get; } = [];

    /// <summary>A cell as the CSV holds it; <c>null</c> for an empty value.</summary>
    public string? Cell(string column) => column switch
    {
        "product_id" => ProductId.ToString(CultureInfo.InvariantCulture),
        "category_id" => CategoryId?.ToString(CultureInfo.InvariantCulture),
        "category_path" => CategoryPath,
        "original_name" => OriginalName,
        "base_name" => BaseName,
        "size" => Size,
        "size_length" => Number(SizeLength),
        "size_width" => Number(SizeWidth),
        "size_thickness" => Number(SizeThickness),
        "size_diameter" => Number(SizeDiameter),
        "color_raw" => ColorRaw,
        "color_code" => ColorCode,
        "color_name" => ColorName,
        "color_secondary_code" => ColorSecondaryCode,
        "color_secondary_name" => ColorSecondaryName,
        "side" => Side,
        "extra_attributes" => ExtraAttributes.ToJsonString(JsonOptions),
        "needs_review" => NeedsReview ? "true" : "false",
        "review_reason" => ReviewReason,
        "notes" => Notes,
        _ => throw new KeyNotFoundException($"Unknown column {column}.")
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static string? Number(decimal? value) => value is { } number ? RecognitionNumbers.Format(number) : null;
}
