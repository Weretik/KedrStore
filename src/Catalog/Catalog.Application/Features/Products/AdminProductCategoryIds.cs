using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products;

internal static class AdminProductCategoryIds
{
    public static async Task<ProductCategoryId[]> ForSelectedTreeAsync(
        IQueryable<ProductCategory> categoriesQuery,
        int selectedCategoryId,
        CancellationToken cancellationToken)
    {
        var categories = await categoriesQuery
            .Select(category => new { category.Id, category.ParentId })
            .ToListAsync(cancellationToken);
        var selectedId = ProductCategoryId.From(selectedCategoryId);
        if (!categories.Any(category => category.Id == selectedId)) return [];

        var childrenByParent = categories
            .Where(category => category.ParentId.HasValue)
            .ToLookup(category => category.ParentId!.Value, category => category.Id);
        var result = new HashSet<ProductCategoryId>();
        var pending = new Stack<ProductCategoryId>();
        pending.Push(selectedId);
        while (pending.TryPop(out var currentId))
        {
            if (!result.Add(currentId)) continue;
            foreach (var childId in childrenByParent[currentId]) pending.Push(childId);
        }

        return [.. result];
    }
}
