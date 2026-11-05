using Catalog.Domain.Errors;

namespace Catalog.Domain.Entities.Attributes;

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

    private static string MaxLength(string value, int maxLength, string field)
    {
        if (value.Length > maxLength)
            throw new DomainException(AttributeErrors.TooLong(field, maxLength, value.Length));

        return value;
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}
