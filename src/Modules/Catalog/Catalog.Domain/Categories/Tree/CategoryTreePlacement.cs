namespace Catalog.Domain.Categories.Tree;

/// <summary>
/// Rebuilds every category's path from its parent (spec catalog/011 MC-05, MC-09): after a move by hand, and after a
/// 1C sync, so a category under a moved parent, a new 1C category under it, and a manual category under a 1C parent
/// that 1C moved all sit where their parent is now.
/// </summary>
public static class CategoryTreePlacement
{
    /// <summary>Returns how many categories changed their place.</summary>
    public static int Rebuild(IReadOnlyCollection<ProductCategory> categories, DateTimeOffset now)
    {
        var byId = categories.ToDictionary(category => category.Id);
        var done = new HashSet<ProductCategoryId>();
        var changed = 0;

        void Settle(ProductCategory category, HashSet<ProductCategoryId> visiting)
        {
            if (done.Contains(category.Id))
                return;
            ProductCategory? parent = null;
            if (category.ParentId is { } parentId && byId.TryGetValue(parentId, out var found) && visiting.Add(parentId))
            {
                Settle(found, visiting);
                parent = found;
            }

            // A missing parent or a loop puts the category at the top instead of failing the whole tree.
            if (category.FollowParent(parent, now))
                changed++;
            done.Add(category.Id);
        }

        foreach (var category in categories.OrderBy(category => category.Level))
            Settle(category, [category.Id]);
        return changed;
    }
}
