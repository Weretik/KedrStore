namespace Catalog.Application.Contracts.Persistence;

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
