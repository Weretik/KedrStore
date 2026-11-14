using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Abstractions.Integrations;
using Catalog.Application.Features.OneCSync.Sync;
using Catalog.Application.Features.OneCSync.Sync.Products;

namespace Catalog.Application.Features.OneCSync.Jobs;

public sealed class SyncOneCStocksJob(
    IOneCClient oneC,
    ICatalogRepository<Product> productRepo,
    IProductListProjectionRebuilder productListProjectionRebuilder,
    ILogger<SyncOneCStocksJob> logger)
{
    private const decimal HighStockWarningThreshold = 1000m;

    public async Task RunAsync(
        string rootCategoryId,
        CancellationToken cancellationToken,
        bool rebuildProjection = true)
    {
        logger.LogInformation("[DEBUG_LOG] SyncOneCStocksJob started for {Root}", rootCategoryId);

        var stocks = await oneC.GetProductStocksAsync(rootCategoryId, cancellationToken);
        logger.LogInformation("[DEBUG_LOG] Received {Count} stock records from 1C for root {Root}", stocks.Count, rootCategoryId);

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

        if (rebuildProjection)
        {
            await productListProjectionRebuilder.RebuildAsync(cancellationToken);
        }

        logger.LogInformation("SyncOneCStocksJob finished for {Root}", rootCategoryId);
    }

    private async Task UpdateStockAsync(
        Dictionary<ProductId, decimal> stocks,
        string productTypeIdOneC,
        CancellationToken cancellationToken)
    {
        var spec = new ProductsByIdsSpec(stocks.Keys, productTypeIdOneC);
        var products = await productRepo.ListAsync(spec, cancellationToken);

        foreach (var product in products)
        {
            if (stocks.TryGetValue(product.Id, out var stock))
            {
                if (stock < 0)
                {
                    logger.LogWarning(
                        "Skipping negative stock received from 1C for ProductId={ProductId}. Stock={Stock}.",
                        product.Id.Value,
                        stock);
                    continue;
                }

                if (stock > HighStockWarningThreshold)
                {
                    logger.LogWarning(
                        "High stock value received from 1C for ProductId={ProductId}. Stock={Stock}.",
                        product.Id.Value,
                        stock);
                }

                var updated = product.UpdateStock(stock);
                if (updated.IsFailure)
                    logger.LogWarning("Skipping stock from 1C for ProductId={ProductId}: {Error}.", product.Id.Value,
                        updated.Error!.Message);
            }
        }

        await productRepo.SaveChangesAsync(cancellationToken);
    }
}
