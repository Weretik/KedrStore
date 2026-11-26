using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Categories.Shared;
using Catalog.Contracts.Category;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.Categories.Shared.Stats;

namespace Catalog.Application.Features.Categories.Queries.GetAdminCategories;

/// <summary>
/// Active rows come depth-first in tree order with product and site model counts (including descendants) and the languages that
/// have site texts; deleted rows come newest first without them.
/// </summary>
public sealed class GetAdminCategoriesHandler(
    ICatalogReadRepository<ProductCategory> repository,
    IAdminCategoryInsightsReader insights)
    : IQueryHandler<GetAdminCategoriesQuery, Result<IReadOnlyList<AdminCategoryResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminCategoryResponse>>> Handle(GetAdminCategoriesQuery query, CancellationToken cancellationToken)
    {
        if (query.Deleted)
        {
            var deleted = await repository.ListAsync(new DeletedCategoriesSpec(), cancellationToken);
            return Result.Success<IReadOnlyList<AdminCategoryResponse>>(deleted.Select(category => CategoryReadMapper.ToAdminResponse(category)).ToList());
        }

        var categories = await repository.ListAsync(new AllCategoriesSpec(null), cancellationToken);
        var directCounts = await insights.LoadDirectProductCountsAsync(cancellationToken);
        var counts = CategoryStatsRollup.RollupCounts(categories, directCounts);
        var languages = await insights.LoadFilledContentLanguagesAsync(cancellationToken);
        var models = await insights.LoadDirectModelCountsAsync(cancellationToken);
        var modelCounts = CategoryStatsRollup.RollupCounts(
            categories, models.ToDictionary(pair => pair.Key, pair => pair.Value.Total));
        var proposedCounts = CategoryStatsRollup.RollupCounts(
            categories, models.ToDictionary(pair => pair.Key, pair => pair.Value.Proposed));
        var byId = categories.ToDictionary(x => x.Id.Value);
        var children = categories.ToLookup(x => x.ParentId?.Value);
        var result = new List<AdminCategoryResponse>();
        var visited = new HashSet<int>();

        void Visit(ProductCategory item)
        {
            if (!visited.Add(item.Id.Value)) return;
            result.Add(CategoryReadMapper.ToAdminResponse(item) with
            {
                ProductCount = counts[item.Id.Value],
                FilledContentLanguages = languages.TryGetValue(item.Id.Value, out var filled) ? filled : [],
                ModelCount = modelCounts[item.Id.Value],
                ProposedModelCount = proposedCounts[item.Id.Value]
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
