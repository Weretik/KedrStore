using Catalog.Application.Contracts.Persistence;

namespace Catalog.Application.Features.Category.Stats;

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
