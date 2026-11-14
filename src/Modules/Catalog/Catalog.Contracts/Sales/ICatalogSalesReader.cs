namespace Catalog.Contracts.Sales;

/// <summary>Catalog data that order taking needs: product names, stock, categories, and prices.</summary>
public interface ICatalogSalesReader
{
    /// <summary>Products that exist among <paramref name="productIds"/>; unknown IDs are skipped.</summary>
    Task<IReadOnlyList<SalesProduct>> GetProductsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken);

    /// <summary>Every category with its parent, for walking up the category tree.</summary>
    Task<IReadOnlyDictionary<int, int?>> GetCategoryParentsAsync(CancellationToken cancellationToken);

    /// <summary>Prices of the products in one currency, every price type.</summary>
    Task<IReadOnlyList<SalesProductPrice>> GetPricesAsync(
        IReadOnlyCollection<int> productIds,
        string currencyIso,
        CancellationToken cancellationToken);
}

public sealed record SalesProduct(int Id, string Name, int CategoryId, decimal Stock);

public sealed record SalesProductPrice(int ProductId, int PriceTypeId, decimal Amount);
