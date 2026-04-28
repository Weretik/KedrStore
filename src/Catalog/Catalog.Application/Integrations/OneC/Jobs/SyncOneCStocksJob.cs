using BuildingBlocks.Domain.Exceptions;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCStocksJob(
    IOneCClient oneC,
    ICatalogRepository<Product> productRepo,
    ILogger<SyncOneCStocksJob> logger)
{
    public async Task RunAsync(string rootCategoryId, CancellationToken cancellationToken)
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

        logger.LogInformation("SyncOneCStocksJob finished for {Root}", rootCategoryId);
    }

    private async Task UpdateStockAsync(
        Dictionary<ProductId, decimal> stocks,
        string productTypeIdOneC,
        CancellationToken cancellationToken)
    {
        var spec = new ProductsByIdsSpec(stocks.Keys, productTypeIdOneC);
        var products = await productRepo.ListAsync(spec, cancellationToken);

        var skipped = 0;

        foreach (var product in products)
        {
            if (!stocks.TryGetValue(product.Id, out var stock))
                continue;

            try
            {
                product.UpdateStock(stock);
            }
            catch (DomainException ex)
            {
                skipped++;
                logger.LogWarning(
                    ex,
                    "Stock update skipped for ProductId={ProductId}. Invalid stock value from 1C: {Stock}.",
                    product.Id.Value,
                    stock);
            }
        }

        await productRepo.SaveChangesAsync(cancellationToken);

        if (skipped > 0)
            logger.LogWarning("SyncOneCStocksJob skipped {SkippedCount} stock updates due to domain validation.", skipped);
    }
}
