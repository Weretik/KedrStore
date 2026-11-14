namespace Catalog.Domain.Groups;

public static class ProductGroupErrors
{
    public static DomainError Required(string field) =>
        new($"Catalog.ProductGroup.{field}.Required", $"{field} is required");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Catalog.ProductGroup.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");
}
