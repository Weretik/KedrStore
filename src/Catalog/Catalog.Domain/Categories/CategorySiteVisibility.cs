namespace Catalog.Domain.Categories;

/// <summary>
/// Spreads a site-visibility change over the category tree (spec catalog/007 R-18). Hiding hides the category and every
/// subcategory; showing shows them and every active parent, so a shown category is always reachable. Deleted
/// subcategories follow, so a category 1C restores keeps its branch's value.
/// </summary>
public static class CategorySiteVisibility
{
    /// <summary>Applies the change to <paramref name="selected"/> and the affected categories; returns how many changed.</summary>
    public static int Apply(ProductCategory selected, IReadOnlyCollection<ProductCategory> tree, bool isShown,
        DateTimeOffset now)
    {
        var prefix = selected.Path.Value + ".";
        var affected = tree
            .Where(category => category.Path.Value.StartsWith(prefix, StringComparison.Ordinal))
            .Prepend(selected);
        if (isShown)
            affected = affected.Concat(ActiveAncestors(selected, tree));

        return affected.Distinct().Count(category => category.SetShownOnSite(isShown, now));
    }

    private static IEnumerable<ProductCategory> ActiveAncestors(ProductCategory category,
        IReadOnlyCollection<ProductCategory> tree)
    {
        var active = tree.Where(item => !item.IsDeleted).ToDictionary(item => item.Id);
        var visited = new HashSet<ProductCategoryId> { category.Id };
        var current = category;
        while (current.ParentId is { } parentId && visited.Add(parentId) && active.TryGetValue(parentId, out var parent))
        {
            yield return parent;
            current = parent;
        }
    }
}
