using Catalog.Application.Features.Products.GetById.DTOs;

namespace Catalog.Application.Features.Products.GetById.Extensions;

public static class ProductCategoryBreadcrumbExtensions
{
    public static async Task<IReadOnlyList<CategoryBreadcrumbDto>> BuildCategoryBreadcrumbsAsync(
        this IQueryable<ProductCategory> categoriesQuery,
        string categorySlug,
        CancellationToken cancellationToken)
    {
        var categoriesMap = await categoriesQuery
            .Select(category => new CategoryForBreadcrumb
            {
                Id = category.Id.Value,
                Name = category.Name,
                Slug = category.Slug,
                ParentId = category.ParentId.HasValue ? category.ParentId.Value.Value : null
            })
            .ToDictionaryAsync(category => category.Id, cancellationToken);

        var current = categoriesMap.Values.FirstOrDefault(category => category.Slug == categorySlug);
        if (current is null)
            return [];

        var breadcrumbs = new List<CategoryBreadcrumbDto>();

        while (current is not null)
        {
            breadcrumbs.Add(new CategoryBreadcrumbDto(
                Id: current.Id,
                Name: current.Name,
                Slug: current.Slug));

            var parentId = current.ParentId;
            if (parentId is null)
                break;

            if (!categoriesMap.TryGetValue(parentId.Value, out current))
                break;
        }

        breadcrumbs.Reverse();
        return breadcrumbs;
    }

    private sealed class CategoryForBreadcrumb
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public string Slug { get; init; } = null!;
        public int? ParentId { get; init; }
    }
}
