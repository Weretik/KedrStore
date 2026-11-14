using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Categories.Shared;

/// <summary>IDs of a selected category and all its descendants among the given categories.</summary>
internal static class CategoryTreeIds
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
