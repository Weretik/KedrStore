namespace Sales.Application.Abstractions.Catalog;

/// <summary>Which of the given category and price type ids Catalog has.</summary>
public interface ICatalogReferenceIds
{
    Task<HashSet<int>> ExistingCategoryIdsAsync(IReadOnlyCollection<int> categoryIds, CancellationToken cancellationToken);

    Task<HashSet<int>> ExistingPriceTypeIdsAsync(IReadOnlyCollection<int> priceTypeIds, CancellationToken cancellationToken);
}
