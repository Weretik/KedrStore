namespace Catalog.Domain.Categories;

public static class CategoryErrors
{
    public static DomainError IdMustBePositive() =>
        new("Catalog.Category.Id.Invalid", "Category id must be greater than zero");

    public static DomainError NameIsRequired() =>
        new("Catalog.Category.Name.Required", "Category name is required");

    public static DomainError SlugIsRequired() =>
        new("Catalog.Category.Slug.Required", "Category slug is required");

    public static DomainError NameLengthInvalid(int length) =>
        new("Catalog.Category.Name.LengthInvalid",
            $"Category name length must be between 1 and 100 characters, Actual:{length}");

    public static DomainError ShortNameUkIsRequired() =>
        new("Catalog.Category.ShortNameUk.Required", "Ukrainian category short name is required");

    public static DomainError ShortNameUkLengthInvalid(int length) =>
        new("Catalog.Category.ShortNameUk.LengthInvalid",
            $"Ukrainian category short name length must be between 1 and 100 characters. Actual:{length}");

    public static DomainError ShortNameRuIsRequired() =>
        new("Catalog.Category.ShortNameRu.Required", "Russian category short name is required");

    public static DomainError ShortNameRuLengthInvalid(int length) =>
        new("Catalog.Category.ShortNameRu.LengthInvalid",
            $"Russian category short name length must be between 1 and 100 characters. Actual:{length}");

    public static DomainError SortOrderNegative(int value) =>
        new("Catalog.Category.SortOrder.Negative",
            $"Category sort order cannot be negative. Actual:{value}");

    public static DomainError LevelNegative(int value) =>
        new("Catalog.Category.Level.Negative",
            $"Category level cannot be negative. Actual:{value}");

    public static DomainError LevelDoesNotMatchPath(int level, int pathLevel) =>
        new("Catalog.Category.Level.PathMismatch",
            $"Category level must match its path depth. Actual:{level}; Expected:{pathLevel}");

    public static DomainError PathIsRequired() =>
        new("Catalog.Category.Path.Required", "Category path is required");

    public static DomainError CannotSetSelfAsParent(string path) =>
        new("Catalog.Category.Reparent.SelfParentForbidden",
            $"You cannot set a node as its own parent. Actual:{path}");

    public static DomainError CannotMoveUnderDescendant(string currentPath, string newParentPath) =>
        new("Catalog.Category.Reparent.CycleForbidden",
            $"You cannot move a node under its own descendant (cycle in the tree). Actual:{currentPath}, {newParentPath}");
}
