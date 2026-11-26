namespace Catalog.Domain.Categories.Tree;

/// <summary>
/// Spreads «Закрити від пошуку» over the category tree (spec catalog/011 R-01): closing or opening a category does the
/// same to every subcategory, deleted ones included, so a branch keeps one value when 1C restores a category.
/// </summary>
public static class CategorySearchClosing
{
    /// <summary>Applies the change to <paramref name="selected"/> and its subcategories; returns how many changed.</summary>
    public static int Apply(ProductCategory selected, IReadOnlyCollection<ProductCategory> tree, bool isClosed,
        DateTimeOffset now)
    {
        var prefix = selected.Path.Value + ".";
        return tree
            .Where(category => category.Path.Value.StartsWith(prefix, StringComparison.Ordinal))
            .Prepend(selected)
            .Distinct()
            .Count(category => category.SetClosedFromSearch(isClosed, now));
    }
}
