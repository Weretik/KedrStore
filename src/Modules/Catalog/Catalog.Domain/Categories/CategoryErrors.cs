using BuildingBlocks.Domain.Abstractions.Errors;

namespace Catalog.Domain.Categories;

public static class CategoryErrors
{
    public static DomainError NameRequired() =>
        new("Catalog.Category.Name.Required", "Заповніть це поле.", Field: "Name");

    public static DomainError NameTooLong(int length) =>
        new("Catalog.Category.Name.LengthInvalid",
            $"Не більше {ProductCategory.NameMaxLength} символів (зараз {length}).", Field: "Name");

    public static DomainError SlugRequired() =>
        new("Catalog.Category.Slug.Required", "Заповніть це поле.", Field: "Slug");

    public static DomainError ShortNameRequired(string field) =>
        new($"Catalog.Category.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError ShortNameTooLong(string field, int length) =>
        new($"Catalog.Category.{field}.LengthInvalid",
            $"Не більше {ProductCategory.ShortNameMaxLength} символів (зараз {length}).", Field: field);

    public static DomainError SortOrderNegative() =>
        new("Catalog.Category.SortOrder.Negative", "Порядок не може бути від'ємним.", Field: "SortOrder");

    public static DomainError LevelNegative() =>
        new("Catalog.Category.Level.Negative", "Рівень не може бути від'ємним.", Field: "Level");

    public static DomainError LevelDoesNotMatchPath(int pathLevel) =>
        new("Catalog.Category.Level.PathMismatch", $"Рівень має відповідати місцю в дереві ({pathLevel}).",
            Field: "Level");

    public static DomainError PathRequired() =>
        new("Catalog.Category.Path.Required", "Потрібне місце категорії в дереві.", Field: "Path");
}
