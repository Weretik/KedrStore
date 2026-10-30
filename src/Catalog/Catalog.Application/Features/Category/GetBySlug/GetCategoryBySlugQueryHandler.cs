using Catalog.Application.Features.Category.Stats;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Category.Content;
using Catalog.Application.Features.Category.GetList;
using Catalog.Application.Features.Category.GetList.DTOs;
using Catalog.Application.Features.Category.GetList.Specifications;
using Catalog.Application.Slugs;
using Catalog.Contracts.Category;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Category.GetBySlug;

/// <summary>
/// Public category page (R-016): a former slug resolves to the current category; a deleted category points to its
/// nearest active ancestor; the page carries counts, the price range, and content in the requested language.
/// </summary>
public sealed class GetCategoryBySlugQueryHandler(
    ICatalogReadRepository<ProductCategory> repository,
    IReadCatalogDbContext catalogDbContext,
    ISlugService slugService,
    ICategoryStatsReader statsReader)
    : IQueryHandler<GetCategoryBySlugQuery, Result<CatalogLookup<CategoryDetailsResponse>>>
{
    public async ValueTask<Result<CatalogLookup<CategoryDetailsResponse>>> Handle(GetCategoryBySlugQuery query,
        CancellationToken cancellationToken)
    {
        var slug = query.Slug.Trim().ToLowerInvariant();
        var categories = await repository.ListAsync(new AllCategoriesSpec(null), cancellationToken);
        var map = categories.ToDictionary(x => x.Id.Value);

        var resolvedId = await slugService.ResolveAsync(CatalogEntityType.Category, slug, cancellationToken)
                         ?? await FindDeletedBySlugAsync(slug, cancellationToken);
        if (resolvedId is null)
            return Result.NotFound();

        if (!map.TryGetValue(resolvedId.Value, out var selected))
        {
            var redirect = await RedirectForDeletedAsync(resolvedId.Value, map, cancellationToken);
            return Result.Success(CatalogLookup<CategoryDetailsResponse>.Gone(redirect));
        }

        var stats = CategoryStatsRollup.Rollup(categories, await statsReader.LoadDirectAsync(cancellationToken));
        var tree = GetCategoriesQuryHandler.BuildTree(categories, query.Lang, stats, hideEmpty: false);
        var node = Find(tree, selected.Id.Value);
        if (node is null)
            return Result.NotFound();

        var content = await catalogDbContext.CategoryContents.AsNoTracking()
            .FirstOrDefaultAsync(item => item.CategoryId == selected.Id && item.Language == query.Lang, cancellationToken);

        var response = CategoryReadMapper.ToResponse(WithoutEmptyChildren(node));
        var selectedStats = stats.GetValueOrDefault(selected.Id.Value, CategoryStats.Empty);
        return Result.Success(CatalogLookup<CategoryDetailsResponse>.Found(new CategoryDetailsResponse(
            response.Id, response.Slug, response.Name, response.ParentId, response.SortOrder,
            response.Level, response.Children, Breadcrumbs(selected, map, query.Lang),
            selectedStats.ProductCount, selectedStats.MinPrice, selectedStats.MaxPrice,
            CategoryContentMapper.ToPublic(content))));
    }

    private Task<int?> FindDeletedBySlugAsync(string slug, CancellationToken cancellationToken)
        => catalogDbContext.Categories.IgnoreQueryFilters().AsNoTracking()
            .Where(category => category.IsDeleted && category.Slug == slug)
            .Select(category => (int?)category.Id.Value)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>Nearest active ancestor of a deleted category, from the deleted category's own path.</summary>
    private async Task<CatalogRedirect?> RedirectForDeletedAsync(int categoryId,
        IReadOnlyDictionary<int, ProductCategory> active, CancellationToken cancellationToken)
    {
        var id = ProductCategoryId.From(categoryId);
        var deleted = await catalogDbContext.Categories.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
        if (deleted is null)
            return null;

        var ancestorIds = deleted.Path.Value.Split('.')
            .Select(segment => int.TryParse(segment.TrimStart('n'), NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : 0)
            .Where(value => value > 0 && value != categoryId)
            .Reverse();

        foreach (var ancestorId in ancestorIds)
        {
            if (active.TryGetValue(ancestorId, out var ancestor))
                return new CatalogRedirect(ancestor.Slug);
        }

        return null;
    }

    private static List<CategoryBreadcrumbResponse> Breadcrumbs(ProductCategory selected,
        IReadOnlyDictionary<int, ProductCategory> map, string lang)
    {
        var breadcrumbs = new List<CategoryBreadcrumbResponse>();
        var current = selected;
        var visited = new HashSet<int>();
        while (visited.Add(current.Id.Value))
        {
            var name = lang == "ru" ? current.ShortNameRu : current.ShortNameUk;
            breadcrumbs.Add(new CategoryBreadcrumbResponse(current.Id.Value, current.Slug, name));
            if (current.ParentId is null || !map.TryGetValue(current.ParentId.Value.Value, out var parent)) break;
            current = parent;
        }

        breadcrumbs.Reverse();
        return breadcrumbs;
    }

    private static CategoryTreeDto WithoutEmptyChildren(CategoryTreeDto node)
        => node with
        {
            Children = node.Children.Where(child => child.ProductCount > 0).Select(WithoutEmptyChildren).ToList()
        };

    private static CategoryTreeDto? Find(IEnumerable<CategoryTreeDto> nodes, int id)
    {
        foreach (var node in nodes)
        {
            if (node.Id == id) return node;
            var found = Find(node.Children, id);
            if (found is not null) return found;
        }
        return null;
    }
}
