using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Category;

public sealed record CategoryStats(int ProductCount, decimal? MinPrice, decimal? MaxPrice)
{
    public static readonly CategoryStats Empty = new(0, null, null);

    public CategoryStats Add(CategoryStats other) => new(
        ProductCount + other.ProductCount,
        Min(MinPrice, other.MinPrice),
        Max(MaxPrice, other.MaxPrice));

    private static decimal? Min(decimal? left, decimal? right)
        => left is null ? right : right is null ? left : Math.Min(left.Value, right.Value);

    private static decimal? Max(decimal? left, decimal? right)
        => left is null ? right : right is null ? left : Math.Max(left.Value, right.Value);
}

public interface ICategoryStatsReader
{
    /// <summary>Exported products and retail price range per category, without descendants.</summary>
    Task<IReadOnlyDictionary<int, CategoryStats>> LoadDirectAsync(CancellationToken cancellationToken);
}

/// <summary>Reads the list projection, which already excludes deleted products and products of deleted categories.</summary>
public sealed class CategoryStatsReader(IReadCatalogDbContext catalogDbContext) : ICategoryStatsReader
{
    public async Task<IReadOnlyDictionary<int, CategoryStats>> LoadDirectAsync(CancellationToken cancellationToken)
    {
        var rows = await catalogDbContext.ProductListProjections.AsNoTracking()
            .Where(projection => projection.ExportToSite)
            .GroupBy(projection => projection.CategoryId)
            .Select(group => new
            {
                CategoryId = group.Key,
                Count = group.Count(),
                Min = group.Min(projection => projection.RetailPrice),
                Max = group.Max(projection => projection.RetailPrice)
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(row => row.CategoryId.Value, row => new CategoryStats(row.Count, row.Min, row.Max));
    }
}

public static class CategoryStatsRollup
{
    /// <summary>Adds each category's direct stats to itself and every ancestor on its path (R-015).</summary>
    public static Dictionary<int, CategoryStats> Rollup(
        IEnumerable<ProductCategory> categories,
        IReadOnlyDictionary<int, CategoryStats> direct)
    {
        var categoryList = categories.ToList();
        var known = categoryList.Select(category => category.Id.Value).ToHashSet();
        var result = categoryList.ToDictionary(category => category.Id.Value, _ => CategoryStats.Empty);

        foreach (var category in categoryList)
        {
            if (!direct.TryGetValue(category.Id.Value, out var stats))
                continue;

            foreach (var ancestorId in PathIds(category.Path))
            {
                if (known.Contains(ancestorId))
                    result[ancestorId] = result[ancestorId].Add(stats);
            }
        }

        return result;
    }

    private static IEnumerable<int> PathIds(CategoryPath path)
        => path.Value.Split('.')
            .Select(segment => segment.TrimStart('n'))
            .Select(segment => int.TryParse(segment, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct();
}
