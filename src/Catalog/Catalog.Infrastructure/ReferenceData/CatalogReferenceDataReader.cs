using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.ValueObjects;

namespace Catalog.Infrastructure.ReferenceData;

internal sealed class CatalogReferenceDataReader(IReadCatalogDbContext catalogDbContext) : ICatalogReferenceDataReader
{
    public async Task<HashSet<int>> GetExistingCategoryIdsAsync(
        IReadOnlyCollection<int> categoryIds,
        CancellationToken cancellationToken)
    {
        if (categoryIds.Count == 0)
            return [];

        var requestedIds = categoryIds
            .Distinct()
            .Select(ProductCategoryId.From)
            .ToArray();

        return await catalogDbContext.Categories
            .AsNoTracking()
            .Where(item => requestedIds.Contains(item.Id))
            .Select(item => item.Id.Value)
            .ToHashSetAsync(cancellationToken);
    }

    public async Task<HashSet<int>> GetExistingPriceTypeIdsAsync(
        IReadOnlyCollection<int> priceTypeIds,
        CancellationToken cancellationToken)
    {
        if (priceTypeIds.Count == 0)
            return [];

        var requestedIds = priceTypeIds
            .Distinct()
            .Select(PriceTypeId.From)
            .ToArray();

        return await catalogDbContext.PriceTypes
            .AsNoTracking()
            .Where(item => requestedIds.Contains(item.Id))
            .Select(item => item.Id.Value)
            .ToHashSetAsync(cancellationToken);
    }
}
