namespace Catalog.Domain.Groups;

public static class ProductGroupErrors
{
    public static DomainError Required(string field) =>
        new($"Catalog.ProductGroup.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Catalog.ProductGroup.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);
}
