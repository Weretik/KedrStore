namespace Catalog.Contracts.Reference;

public interface ICatalogReferenceDataReader
{
    Task<HashSet<int>> GetExistingCategoryIdsAsync(
        IReadOnlyCollection<int> categoryIds,
        CancellationToken cancellationToken);

    Task<HashSet<int>> GetExistingPriceTypeIdsAsync(
        IReadOnlyCollection<int> priceTypeIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, string>> GetCategoryNamesAsync(
        IReadOnlyCollection<int> categoryIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, string>> GetPriceTypeNamesAsync(
        IReadOnlyCollection<int> priceTypeIds,
        CancellationToken cancellationToken);
}
