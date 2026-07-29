using System.Globalization;

namespace Catalog.Domain.Errors;

public static class CategoryErrors
{
    public static CatalogDomainError IdMustBePositive() =>
        new("Catalog.Category.Id.Invalid", "Category id must be greater than zero");

    public static CatalogDomainError NameIsRequired() =>
        new("Catalog.Category.Name.Required", "Category name is required");

    public static CatalogDomainError SlugIsRequired() =>
        new("Catalog.Category.Slug.Required", "Category slug is required");

    public static CatalogDomainError NameLengthInvalid(int length) =>
        new("Catalog.Category.Name.LengthInvalid",
            $"Category name length must be between 1 and 100 characters, Actual:{length}");

    public static CatalogDomainError ShortNameUkIsRequired() =>
        new("Catalog.Category.ShortNameUk.Required", "Ukrainian category short name is required");

    public static CatalogDomainError ShortNameUkLengthInvalid(int length) =>
        new("Catalog.Category.ShortNameUk.LengthInvalid",
            $"Ukrainian category short name length must be between 1 and 100 characters. Actual:{length}");

    public static CatalogDomainError ShortNameRuIsRequired() =>
        new("Catalog.Category.ShortNameRu.Required", "Russian category short name is required");

    public static CatalogDomainError ShortNameRuLengthInvalid(int length) =>
        new("Catalog.Category.ShortNameRu.LengthInvalid",
            $"Russian category short name length must be between 1 and 100 characters. Actual:{length}");

    public static CatalogDomainError SortOrderNegative(int value) =>
        new("Catalog.Category.SortOrder.Negative",
            $"Category sort order cannot be negative. Actual:{value}");

    public static CatalogDomainError LevelNegative(int value) =>
        new("Catalog.Category.Level.Negative",
            $"Category level cannot be negative. Actual:{value}");

    public static CatalogDomainError LevelDoesNotMatchPath(int level, int pathLevel) =>
        new("Catalog.Category.Level.PathMismatch",
            $"Category level must match its path depth. Actual:{level}; Expected:{pathLevel}");

    public static CatalogDomainError PathIsRequired() =>
        new("Catalog.Category.Path.Required", "Category path is required");

    public static CatalogDomainError CannotSetSelfAsParent(string path) =>
        new("Catalog.Category.Reparent.SelfParentForbidden",
            $"You cannot set a node as its own parent. Actual:{path}");

    public static CatalogDomainError CannotMoveUnderDescendant(string currentPath, string newParentPath) =>
        new("Catalog.Category.Reparent.CycleForbidden",
            $"You cannot move a node under its own descendant (cycle in the tree). Actual:{currentPath}, {newParentPath}");
}
