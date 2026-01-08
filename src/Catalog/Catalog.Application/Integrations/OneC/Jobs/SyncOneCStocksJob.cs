using BuildingBlocks.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Application.Persistance;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCStocksJob(IOneCClient oneC, ICatalogRepository<Product> productRepo)
{
    public async Task RunAsync(string rootCategoryId, CancellationToken cancellationToken)
    {
        var stocks = await oneC.GetProductStocksAsync(rootCategoryId, cancellationToken);

        if (stocks.Count == 0)
            return;

        var stockByProductId = stocks
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToDictionary(
                x => ProductId.From(int.Parse(x.Id.TrimStart('0'))),
                x => x.Stock
            );

        if (stockByProductId.Count == 0)
            return;

        await UpdateStockAsync(stockByProductId, cancellationToken);
    }

    private async Task UpdateStockAsync(Dictionary<ProductId, decimal> stocks, CancellationToken cancellationToken)
    {
        var spec = new ProductsByIdsSpec(stocks.Keys);
        var products = await productRepo.ListAsync(spec, cancellationToken);

        foreach (var product in products)
        {
            if (stocks.TryGetValue(product.Id, out var stock))
            {
                product.UpdateStock(stock);
            }
        }

        await productRepo.SaveChangesAsync(cancellationToken);
    }

}
