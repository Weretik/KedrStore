using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Categories.Queries.GetCategories;
using Catalog.Application.Features.Categories.Shared;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Queries.GetAdminCategories;

/// <summary>
/// Active rows come depth-first in tree order with product counts (including descendants) and the languages that
/// have site texts; deleted rows come newest first without them.
/// </summary>
public sealed class GetAdminCategoriesQueryHandler(
    ICatalogReadRepository<ProductCategory> repository,
    IAdminCategoryInsightsReader insights)
    : IQueryHandler<GetAdminCategoriesQuery, Result<IReadOnlyList<AdminCategoryResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminCategoryResponse>>> Handle(GetAdminCategoriesQuery query, CancellationToken cancellationToken)
    {
        if (query.Deleted)
        {
            var deleted = await repository.ListAsync(new DeletedCategoriesSpec(), cancellationToken);
            return Result.Success<IReadOnlyList<AdminCategoryResponse>>(deleted.Select(CategoryReadMapper.ToAdminResponse).ToList());
        }

        var categories = await repository.ListAsync(new AllCategoriesSpec(null), cancellationToken);
        var directCounts = await insights.LoadDirectProductCountsAsync(cancellationToken);
        var counts = CategoryStatsRollup.Rollup(
            categories,
            directCounts.ToDictionary(pair => pair.Key, pair => new CategoryStats(pair.Value, null, null)));
        var languages = await insights.LoadFilledContentLanguagesAsync(cancellationToken);
        var byId = categories.ToDictionary(x => x.Id.Value);
        var children = categories.ToLookup(x => x.ParentId?.Value);
        var result = new List<AdminCategoryResponse>();
        var visited = new HashSet<int>();

        void Visit(ProductCategory item)
        {
            if (!visited.Add(item.Id.Value)) return;
            result.Add(CategoryReadMapper.ToAdminResponse(item) with
            {
                ProductCount = counts.TryGetValue(item.Id.Value, out var stats) ? stats.ProductCount : 0,
                FilledContentLanguages = languages.TryGetValue(item.Id.Value, out var filled) ? filled : []
            });
            foreach (var child in children[item.Id.Value].OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value)) Visit(child);
        }

        foreach (var root in categories.Where(x => x.ParentId is null || !byId.ContainsKey(x.ParentId.Value.Value))
                     .OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value)) Visit(root);
        foreach (var orphan in categories.OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value)) Visit(orphan);
        return Result.Success<IReadOnlyList<AdminCategoryResponse>>(result);
    }
}

internal sealed class DeletedCategoriesSpec : Specification<ProductCategory>
{
    public DeletedCategoriesSpec()
        => Query.AsNoTracking().IgnoreQueryFilters().Where(category => category.IsDeleted)
            .OrderByDescending(category => category.DeletedAt).ThenBy(category => category.Id);
}
