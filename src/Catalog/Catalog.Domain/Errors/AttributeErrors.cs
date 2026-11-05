namespace Catalog.Domain.Errors;

public static class AttributeErrors
{
    public static CatalogDomainError Required(string field) =>
        new($"Catalog.Attribute.{field}.Required", $"{field} is required");

    public static CatalogDomainError TooLong(string field, int maxLength, int length) =>
        new($"Catalog.Attribute.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static CatalogDomainError SlugInvalid(string slug) =>
        new("Catalog.Attribute.Slug.Invalid", $"Slug must match ^[a-z0-9]+(-[a-z0-9]+)*$. Actual: {slug}");

    public static CatalogDomainError SwatchHexInvalid(string value) =>
        new("Catalog.AttributeValue.SwatchHex.Invalid", $"Swatch color must be #RRGGBB. Actual: {value}");

    public static CatalogDomainError SwatchMediaInvalid(long id) =>
        new("Catalog.AttributeValue.SwatchMedia.Invalid", $"Swatch picture must be a media library file ID. Actual: {id}");

    public static CatalogDomainError ValueOrNumberRequired() =>
        new("Catalog.ProductAttribute.Value.Required", "A product attribute needs a list value or a number");

    public static CatalogDomainError PositionInvalid(int position) =>
        new("Catalog.ProductAttribute.Position.Invalid", $"Position must be between 0 and 20. Actual: {position}");
}
