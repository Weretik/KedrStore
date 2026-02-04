using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCPricesJob(
    IOneCClient oneC,
    ICatalogRepository<ProductPrice> priceRepo,
    ICatalogRepository<Product> productRepo,
    ILogger<SyncOneCPricesJob> logger)
{
    public async Task RunAsync(string rootCategoryOneCId, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DEBUG_LOG] SyncOneCPricesJob started for {Root}", rootCategoryOneCId);

        var pricesOneC = await oneC.GetProductPricesAsync(rootCategoryOneCId, cancellationToken);
        logger.LogInformation("[DEBUG_LOG] Received {Count} prices from 1C for root {Root}", pricesOneC.Count, rootCategoryOneCId);

        if (pricesOneC.Count == 0)
            return;

        // FIX #1: убираем дубли по (ProductId, PriceTypeId), чтобы не словить 23505 на SaveChanges
        var deduped = pricesOneC
            .GroupBy(x => (x.ProductId, x.PriceTypeId))
            .Select(g => g.Last())
            .ToList();

        await DeletePricesMissingAsync(deduped, rootCategoryOneCId, cancellationToken);
        await CreateOrUpsertPricesAsync(deduped, rootCategoryOneCId, cancellationToken);

        logger.LogInformation("SyncOneCPricesJob finished for {Root}", rootCategoryOneCId);
    }

    private async Task DeletePricesMissingAsync(
        IReadOnlyList<OneCPriceDto> priceDtos,
        string productTypeIdOneC,
        CancellationToken cancellationToken)
    {
        var productIds = priceDtos
            .Select(x => ProductId.From(x.ProductId))
            .Distinct()
            .ToArray();

        var keepKeys = priceDtos
            .Select(x => (
                ProductTypeIdOneC: productTypeIdOneC,
                ProductId: ProductId.From(x.ProductId),
                PriceTypeId: PriceTypeId.From(x.PriceTypeId))
            )
            .ToHashSet();

        var existing = await priceRepo.ListAsync(
            new PricesByProductIdsSpec(productIds, productTypeIdOneC),
            cancellationToken);

        var toDelete = existing
            .Where(p => !keepKeys.Contains((p.ProductTypeIdOneC, p.ProductId, p.PriceTypeId)))
            .ToList();

        if (toDelete.Count == 0) return;

        await priceRepo.DeleteRangeAsync(toDelete, cancellationToken);
    }

    private async Task CreateOrUpsertPricesAsync(
        IReadOnlyList<OneCPriceDto> priceDtos,
        string productTypeIdOneC,
        CancellationToken cancellationToken)
    {
        foreach (var item in priceDtos)
        {
            var productId = ProductId.From(item.ProductId);
            var priceTypeId = PriceTypeId.From(item.PriceTypeId);
            var priceValue = new Money(item.Price, item.Currency);

            // FIX #2: защита от FK (23503) — если товара нет, цену не пишем
            var product = await productRepo.FirstOrDefaultAsync(
                new ProductByIdSpec(productId),
                cancellationToken);

            if (product is null) continue;

            var existing = await priceRepo.FirstOrDefaultAsync(
                new ProductPriceByProductAndTypeSpec(productId, priceTypeId),
                cancellationToken);

            if (existing is null)
            {
                var price = ProductPrice.Create(
                    productTypeIdOneC: productTypeIdOneC,
                    productId: productId,
                    priceTypeId: priceTypeId,
                    price: priceValue);

                await priceRepo.AddAsync(price, cancellationToken);
            }
            else
            {
                existing.Update(
                    productId: productId,
                    priceTypeId: priceTypeId,
                    price: priceValue);
            }
        }

        await priceRepo.SaveChangesAsync(cancellationToken);
    }
}
