using Catalog.Application.Features.Categories.Shared;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.Categories.Shared.Stats;

namespace Catalog.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Public tree: each node carries its product count; empty branches and hidden import roots (Cosmos) are omitted
/// (R-015).
/// </summary>
public class GetCategoriesHandler(
    ICatalogReadRepository<ProductCategory> categoryRepository,
    ICategoryStatsReader statsReader,
    IOptions<RootCategoryId> roots)
    : IQueryHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryTreeDto>>>
{
    public async ValueTask<Result<IReadOnlyList<CategoryTreeDto>>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var categories = (await categoryRepository.ListAsync(new AllCategoriesSpec(query.Filter.ProductTypeId), cancellationToken))
            .Where(category => !PublicCatalogScope.IsHidden(category, roots.Value))
            .ToList();
        var stats = CategoryStatsRollup.Rollup(categories, await statsReader.LoadDirectAsync(cancellationToken));
        IReadOnlyList<CategoryTreeDto> tree = CategoryTree.Build(categories, query.Lang, stats, hideEmpty: true);
        return Result.Success(tree);
    }
}
