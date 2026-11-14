namespace Catalog.Domain.Attributes;

public static class AttributeErrors
{
    public static DomainError Required(string field) =>
        new($"Catalog.Attribute.{field}.Required", $"{field} is required");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Catalog.Attribute.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static DomainError SlugInvalid(string slug) =>
        new("Catalog.Attribute.Slug.Invalid", $"Slug must match ^[a-z0-9]+(-[a-z0-9]+)*$. Actual: {slug}");

    public static DomainError SwatchHexInvalid(string value) =>
        new("Catalog.AttributeValue.SwatchHex.Invalid", $"Swatch color must be #RRGGBB. Actual: {value}");

    public static DomainError SwatchMediaInvalid(long id) =>
        new("Catalog.AttributeValue.SwatchMedia.Invalid", $"Swatch picture must be a media library file ID. Actual: {id}");

    public static DomainError ValueOrNumberRequired() =>
        new("Catalog.ProductAttribute.Value.Required", "A product attribute needs a list value or a number");

    public static DomainError PositionInvalid(int position) =>
        new("Catalog.ProductAttribute.Position.Invalid", $"Position must be between 0 and 20. Actual: {position}");
}
