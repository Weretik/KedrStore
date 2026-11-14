namespace Catalog.Domain.Attributes;

/// <summary>Shared checks for the text fields of attributes, values, and aliases.</summary>
internal static partial class AttributeText
{
    public static string Required(string? value, int maxLength, string field)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException(AttributeErrors.Required(field));

        return MaxLength(trimmed, maxLength, field);
    }

    public static string? Optional(string? value, int maxLength, string field)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : MaxLength(trimmed, maxLength, field);
    }

    public static string Slug(string? slug)
    {
        var value = Required(slug, 80, "Slug");
        if (!SlugPattern().IsMatch(value))
            throw new DomainException(AttributeErrors.SlugInvalid(value));

        return value;
    }

    /// <summary>An optional <c>#RRGGBB</c> color, stored in upper case.</summary>
    public static string? SwatchHex(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        if (!HexPattern().IsMatch(trimmed))
            throw new DomainException(AttributeErrors.SwatchHexInvalid(trimmed));

        return trimmed.ToUpperInvariant();
    }

    private static string MaxLength(string value, int maxLength, string field)
    {
        if (value.Length > maxLength)
            throw new DomainException(AttributeErrors.TooLong(field, maxLength, value.Length));

        return value;
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexPattern();
}
