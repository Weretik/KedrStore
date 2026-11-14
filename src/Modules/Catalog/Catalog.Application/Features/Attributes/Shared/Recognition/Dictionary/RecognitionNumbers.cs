using System.Text.Json.Nodes;
namespace Catalog.Application.Features.Attributes.Shared.Recognition.Dictionary;

/// <summary>Numbers as the Python recognizer writes them: «2,5» is 2.5, «25.0» is 25.</summary>
internal static class RecognitionNumbers
{
    public static decimal Parse(string text)
        => Normalize(decimal.Parse(text.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture));

    /// <summary>Drops trailing zeros: 25.0 → 25, 2.50 → 2.5.</summary>
    public static decimal Normalize(decimal value) => value / 1.0000000000000000000000000000m;

    public static string Format(decimal value) => Normalize(value).ToString(CultureInfo.InvariantCulture);

    public static JsonNode Json(decimal value)
    {
        var normalized = Normalize(value);
        return normalized == decimal.Truncate(normalized) && Math.Abs(normalized) < long.MaxValue
            ? JsonValue.Create((long)normalized)
            : JsonValue.Create(normalized);
    }

    public static JsonNode? Json(decimal? value) => value is { } number ? Json(number) : null;

    public static JsonArray JsonArray(IEnumerable<decimal> values) => new(values.Select(value => (JsonNode?)Json(value)).ToArray());

    public static JsonArray? JsonArray(IEnumerable<decimal?>? values)
        => values is null ? null : new JsonArray(values.Select(Json).ToArray());
}
