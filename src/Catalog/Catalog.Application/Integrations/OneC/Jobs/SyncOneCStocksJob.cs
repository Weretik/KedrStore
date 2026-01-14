using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Application.Persistance;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCStocksJob(IOneCClient oneC, ICatalogRepository<Product> productRepo,
    ILogger<SyncOneCStocksJob> logger)
{
    [DisableConcurrentExecution(60 * 60)]
    public async Task RunAsync(string rootCategoryId, CancellationToken cancellationToken)
    {
        logger.LogInformation("SyncOneCStocksJob started for {Root}", rootCategoryId);

        var stocks = await oneC.GetProductStocksAsync(rootCategoryId, cancellationToken);

        if (stocks.Count == 0)
            return;

        var stockByProductId = stocks
            .ToDictionary(
                x => ProductId.From(x.Id),
                x => x.Stock
            );

        if (stockByProductId.Count == 0)
            return;

        await UpdateStockAsync(stockByProductId, rootCategoryId, cancellationToken);

        logger.LogInformation("SyncOneCStocksJob finished for {Root}", rootCategoryId);
    }

    private async Task UpdateStockAsync(Dictionary<ProductId, decimal> stocks, string productTypeIdOneC, CancellationToken cancellationToken)
    {
        var spec = new ProductsByIdsSpec(stocks.Keys, productTypeIdOneC );
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
