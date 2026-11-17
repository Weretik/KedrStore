using Catalog.Contracts.Reference;
using Sales.Application.Abstractions.Catalog;

namespace Sales.Infrastructure.Adapters.Catalog;

internal sealed class CatalogReferenceIds(ICatalogReferenceDataReader catalog) : ICatalogReferenceIds
{
    public Task<HashSet<int>> ExistingCategoryIdsAsync(IReadOnlyCollection<int> categoryIds, CancellationToken cancellationToken)
        => catalog.GetExistingCategoryIdsAsync(categoryIds, cancellationToken);

    public Task<HashSet<int>> ExistingPriceTypeIdsAsync(IReadOnlyCollection<int> priceTypeIds, CancellationToken cancellationToken)
        => catalog.GetExistingPriceTypeIdsAsync(priceTypeIds, cancellationToken);
}
