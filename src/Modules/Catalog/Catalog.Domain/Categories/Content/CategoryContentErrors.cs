using BuildingBlocks.Domain.Abstractions.Errors;

namespace Catalog.Domain.Categories.Content;

public static class CategoryContentErrors
{
    public static DomainError Required(string field) =>
        new($"Catalog.CategoryContent.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Catalog.CategoryContent.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).",
            Field: field);

    public static DomainError TooManyFaqItems(int count) =>
        new("Catalog.CategoryContent.Faq.TooMany",
            $"Не більше {CategoryContent.FaqMaxItems} запитань (зараз {count}).", Field: "Faq");
}
