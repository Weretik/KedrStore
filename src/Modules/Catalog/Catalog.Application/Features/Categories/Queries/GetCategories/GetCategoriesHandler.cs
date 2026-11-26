using Catalog.Application.Shared;
using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.Categories.Shared;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.Categories.Shared.Stats;

namespace Catalog.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Public tree: each node carries its product count; empty branches and hidden import roots (Cosmos) are omitted
/// (R-015). The tree is read through <see cref="CategoryTreeCache"/>.
/// </summary>
public class GetCategoriesHandler(
    ICatalogReadRepository<ProductCategory> categoryRepository,
    ICategoryStatsReader statsReader,
    IOptions<RootCategoryId> roots,
    IReadCache cache)
    : IQueryHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryTreeDto>>>
{
    public async ValueTask<Result<IReadOnlyList<CategoryTreeDto>>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var language = CatalogLanguage.Normalize(query.Lang);
        var snapshot = await cache.GetOrCreateAsync(
            CategoryTreeCache.Entry(language, query.Filter.ProductTypeId), token => BuildAsync(query.Filter, language, token), cancellationToken);
        return Result.Success(snapshot.Roots);
    }

    private async ValueTask<CategoryTreeSnapshot> BuildAsync(CategoryFilter filter, string language, CancellationToken cancellationToken)
    {
        var categories = (await categoryRepository.ListAsync(new AllCategoriesSpec(filter.ProductTypeId), cancellationToken))
            .Where(category => !PublicCatalogScope.IsHidden(category, roots.Value))
            .ToList();
        var stats = CategoryStatsRollup.Rollup(categories, await statsReader.LoadDirectAsync(cancellationToken));
        // A text edit changes the page too, so the sitemap's lastmod takes the later date (spec catalog/010 R-04).
        var contentChanges = await statsReader.LoadContentChangesAsync(language, cancellationToken);
        return new CategoryTreeSnapshot(CategoryTree.Build(categories, language, stats, hideEmpty: true, contentChanges));
    }
}
