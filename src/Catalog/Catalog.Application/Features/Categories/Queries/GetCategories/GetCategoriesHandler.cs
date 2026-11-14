using Catalog.Application.Features.Categories.Shared;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.OneCSync.Mapping;

namespace Catalog.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Public tree: each node carries its product count; empty branches and hidden import roots (Cosmos) are omitted
/// (R-015).
/// </summary>
public class GetCategoriesQuryHandler(
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
        IReadOnlyList<CategoryTreeDto> tree = BuildTree(categories, query.Lang, stats, hideEmpty: true);
        return Result.Success(tree);
    }

    internal static IReadOnlyList<CategoryTreeDto> BuildTree(
        IEnumerable<ProductCategory> categories,
        string lang,
        IReadOnlyDictionary<int, CategoryStats> stats,
        bool hideEmpty)
    {
        int CountOf(int id) => stats.TryGetValue(id, out var value) ? value.ProductCount : 0;

        var items = categories.ToList()
            .Select(c => new
            {
                Id = c.Id.Value,
                c.Slug,
                c.Name,
                ParentId = c.ParentId?.Value,
                c.ShortNameUk,
                c.ShortNameRu,
                c.SortOrder,
                c.Level,
                Path = c.Path.Value
            })
            .ToList();

        static string? ParentKey(string path)
        {
            var i = path.LastIndexOf('.');
            return i >= 0 ? path[..i] : null;
        }
        var byPath = items.ToDictionary(i => i.Path, StringComparer.Ordinal);
        var lookup = items.ToLookup(i => ParentKey(i.Path));

        IReadOnlyList<CategoryTreeDto> BuildBranch(string? parentKey, HashSet<string> ancestors)
        {
            return lookup[parentKey]
                .Where(i => !hideEmpty || CountOf(i.Id) > 0)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .Select(i => new CategoryTreeDto(
                    Id: i.Id,
                    Slug: i.Slug,
                    Name: string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase) ? i.ShortNameRu : i.ShortNameUk,
                    ParentId: i.ParentId ?? (ParentKey(i.Path) is { } parentPath && byPath.TryGetValue(parentPath, out var parent) ? parent.Id : null),
                    ShortNameUk: i.ShortNameUk,
                    ShortNameRu: i.ShortNameRu,
                    SortOrder: i.SortOrder,
                    Level: i.Level,
                    Children: ancestors.Contains(i.Path)
                        ? []
                        : BuildBranch(i.Path, new HashSet<string>(ancestors, StringComparer.Ordinal) { i.Path }),
                    ProductCount: CountOf(i.Id)
                ))
                .ToList();
        }

        return BuildBranch(null, new HashSet<string>(StringComparer.Ordinal));
    }
}
