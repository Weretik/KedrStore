using System.Globalization;

namespace Catalog.Domain.Errors;

public static class CategoryErrors
{
    public static DomainError IdMustBePositive() =>
        new("Catalog.Category.Id.Invalid", "Category id must be greater than zero");

    public static DomainError NameIsRequired() =>
        new("Catalog.Category.Name.Required", "Category name is required");

    public static DomainError SlugIsRequired() =>
        new("Category.SlugIsRequired", "Category slug is required");

    public static DomainError NameLengthInvalid(int length) =>
        new("Catalog.Category.Name.LengthInvalid",
            $"Category name length must be between 1 and 100 characters, Actual:{length}");

    public static DomainError PathIsRequired() =>
        new("Catalog.Category.Path.Required", "Category path is required");

    public static DomainError CannotSetSelfAsParent(string path) =>
        new("Catalog.Category.Reparent.SelfParentForbidden",
            $"You cannot set a node as its own parent. Actual:{path}");

    public static DomainError CannotMoveUnderDescendant(string currentPath, string newParentPath) =>
        new("Catalog.Category.Reparent.CycleForbidden",
            $"You cannot move a node under its own descendant (cycle in the tree). Actual:{currentPath}, {newParentPath}");
}
