using Catalog.Application.Abstractions.Persistence;
using Catalog.Contracts.Sales;
using Catalog.Domain.Products;

namespace Catalog.Infrastructure.Contracts.Sales;

internal sealed class CatalogSalesReader(IReadCatalogDbContext catalogDb) : ICatalogSalesReader
{
    public async Task<IReadOnlyList<SalesProduct>> GetProductsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
            return [];

        var ids = productIds.Distinct().Select(ProductId.From).ToArray();
        var products = await catalogDb.Products.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.CategoryId, x.Stock })
            .ToListAsync(cancellationToken);
        return products.Select(x => new SalesProduct(x.Id.Value, x.Name, x.CategoryId.Value, x.Stock)).ToArray();
    }

    public async Task<IReadOnlyDictionary<int, int?>> GetCategoryParentsAsync(CancellationToken cancellationToken)
        => (await catalogDb.Categories.AsNoTracking()
                .Select(x => new { x.Id, x.ParentId })
                .ToListAsync(cancellationToken))
            .ToDictionary(x => x.Id.Value, x => x.ParentId?.Value);

    public async Task<IReadOnlyList<SalesProductPrice>> GetPricesAsync(
        IReadOnlyCollection<int> productIds,
        string currencyIso,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
            return [];

        var ids = productIds.Distinct().Select(ProductId.From).ToArray();
        var prices = await catalogDb.ProductPrices.AsNoTracking()
            .Where(x => ids.Contains(x.ProductId))
            .Select(x => new { x.ProductId, x.PriceTypeId, x.Amount, x.CurrencyIso })
            .ToListAsync(cancellationToken);
        return prices
            .Where(x => x.CurrencyIso == currencyIso)
            .Select(x => new SalesProductPrice(x.ProductId.Value, x.PriceTypeId.Value, x.Amount))
            .ToArray();
    }
}
