namespace Catalog.Domain.Attributes;

/// <summary>Shared checks for the text fields of attributes, values, and aliases.</summary>
internal static partial class AttributeText
{
    public static DomainResult<string> Required(string? value, int maxLength, string field)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return AttributeErrors.Required(field);

        return MaxLength(trimmed, maxLength, field);
    }

    public static DomainResult<string?> Optional(string? value, int maxLength, string field)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return (string?)null;
        var checkedValue = MaxLength(trimmed, maxLength, field);
        return checkedValue.IsFailure ? checkedValue.Error! : checkedValue.Value;
    }

    public static DomainResult<string> Slug(string? slug)
    {
        var value = Required(slug, 80, "Slug");
        if (value.IsFailure)
            return value;
        return SlugPattern().IsMatch(value.Value) ? value : AttributeErrors.SlugInvalid();
    }

    /// <summary>An optional <c>#RRGGBB</c> color, stored in upper case.</summary>
    public static DomainResult<string?> SwatchHex(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return (string?)null;
        return HexPattern().IsMatch(trimmed) ? trimmed.ToUpperInvariant() : AttributeErrors.SwatchHexInvalid();
    }

    /// <summary>
    /// The value of a check whose input the code itself built (recognition results, the signed-in editor), where a
    /// failure is a bug rather than user input.
    /// </summary>
    public static T Guarded<T>(DomainResult<T> result, string field)
        => result.IsSuccess ? result.Value : throw new ArgumentException(result.Error!.Message, field);

    public static void Guarded(DomainResult result, string field)
    {
        if (result.IsFailure)
            throw new ArgumentException(result.Error!.Message, field);
    }

    private static DomainResult<string> MaxLength(string value, int maxLength, string field)
        => value.Length > maxLength ? AttributeErrors.TooLong(field, maxLength, value.Length) : value;

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexPattern();
}
