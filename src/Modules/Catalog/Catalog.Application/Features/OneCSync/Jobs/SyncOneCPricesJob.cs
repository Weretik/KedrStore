using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Abstractions.Integrations;
using Catalog.Application.Features.OneCSync.Source;
using Catalog.Application.Features.OneCSync.Sync;
using BuildingBlocks.Domain.Abstractions.Errors;
using Catalog.Application.Features.OneCSync.Source.Prices;

namespace Catalog.Application.Features.OneCSync.Jobs;

/// <summary>
/// Brings the prices of one 1C root in line with 1C. It reads the products and prices it needs in two queries and saves
/// once, so a price list is replaced as a whole (database rules 1.2 and 3.1).
/// </summary>
public sealed class SyncOneCPricesJob(
    IOneCClient oneC,
    ICatalogRepository<ProductPrice> priceRepo,
    IReadCatalogDbContext db,
    IProductListProjectionRebuilder productListProjectionRebuilder,
    ILogger<SyncOneCPricesJob> logger)
{
    public async Task RunAsync(
        string rootCategoryOneCId,
        CancellationToken cancellationToken,
        bool rebuildProjection = true)
    {
        var pricesOneC = await oneC.GetProductPricesAsync(rootCategoryOneCId, cancellationToken);
        logger.LogInformation("Received {Count} prices from 1C for root {Root}", pricesOneC.Count, rootCategoryOneCId);

        if (pricesOneC.Count == 0)
            return;

        // 1C may send one price twice; the last one wins, so the unique (product, price type) index holds.
        var deduped = pricesOneC
            .GroupBy(x => (x.ProductId, x.PriceTypeId))
            .Select(g => g.Last())
            .ToList();

        var productIds = deduped.Select(x => ProductId.Create(x.ProductId)).Distinct().ToArray();
        // Tracked on purpose: the prices found here are updated or removed and saved below.
        var existing = await db.ProductPrices
            .Where(price => productIds.Contains(price.ProductId))
            .ToDictionaryAsync(price => (price.ProductId, price.PriceTypeId), cancellationToken);

        var removed = RemoveMissing(deduped, existing.Values, rootCategoryOneCId);
        var (added, changed) = await AddOrUpdateAsync(deduped, existing, productIds, rootCategoryOneCId, cancellationToken);
        await priceRepo.SaveChangesAsync(cancellationToken);

        if (rebuildProjection)
            await productListProjectionRebuilder.RebuildAsync(cancellationToken);

        logger.LogInformation(
            "SyncOneCPricesJob finished for {Root}: added {Added}, changed {Changed}, removed {Removed}",
            rootCategoryOneCId, added, changed, removed);
    }

    /// <summary>Removes this root's prices of the received products that 1C no longer sends.</summary>
    private int RemoveMissing(IReadOnlyList<OneCPriceDto> priceDtos, IEnumerable<ProductPrice> existing,
        string productTypeIdOneC)
    {
        var keepKeys = priceDtos
            .Select(x => (ProductId.Create(x.ProductId), PriceTypeId.Create(x.PriceTypeId)))
            .ToHashSet();

        var toDelete = existing
            .Where(p => p.ProductTypeIdOneC == productTypeIdOneC && !keepKeys.Contains((p.ProductId, p.PriceTypeId)))
            .ToList();

        foreach (var price in toDelete)
            db.ProductPrices.Remove(price);
        return toDelete.Count;
    }

    private async Task<(int Added, int Changed)> AddOrUpdateAsync(IReadOnlyList<OneCPriceDto> priceDtos,
        IReadOnlyDictionary<(ProductId, PriceTypeId), ProductPrice> existing, ProductId[] productIds,
        string productTypeIdOneC, CancellationToken cancellationToken)
    {
        // A price of a product that is not in the catalogue would break the foreign key, so it is skipped.
        var knownProducts = (await db.Products.AsNoTracking()
                .Where(product => productIds.Contains(product.Id))
                .Select(product => product.Id)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var (added, changed) = (0, 0);
        foreach (var item in priceDtos)
        {
            var productId = ProductId.Create(item.ProductId);
            if (!knownProducts.Contains(productId)) continue;

            var priceTypeId = PriceTypeId.Create(item.PriceTypeId);
            var priceValue = new Money(item.Price, item.Currency);

            if (existing.TryGetValue((productId, priceTypeId), out var price))
            {
                var updated = price.Update(productId: productId, priceTypeId: priceTypeId, price: priceValue);
                if (updated.IsFailure)
                    LogSkipped(productId, updated.Error!);
                else
                    changed++;
                continue;
            }

            var created = ProductPrice.Create(
                productTypeIdOneC: productTypeIdOneC,
                productId: productId,
                priceTypeId: priceTypeId,
                price: priceValue);
            if (created.IsFailure)
            {
                LogSkipped(productId, created.Error!);
                continue;
            }

            await priceRepo.AddAsync(created.Value, cancellationToken);
            added++;
        }

        return (added, changed);
    }

    private void LogSkipped(ProductId productId, DomainError error)
        => logger.LogWarning("Skipping a price from 1C for ProductId={ProductId}: {Error}", productId.Value, error.Message);
}
