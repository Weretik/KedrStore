using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Application.Persistance;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCPricesJob(IOneCClient oneC, ICatalogRepository<ProductPrice> priceRepo,
    ILogger<SyncOneCPricesJob> logger)
{
    [DisableConcurrentExecution(60 * 60 * 2)]
    public async Task RunAsync(string rootCategoryOneCId, CancellationToken cancellationToken)
    {
        logger.LogInformation("SyncOneCPricesJob started for {Root}", rootCategoryOneCId);

        var pricesOneC = await oneC.GetProductPricesAsync(rootCategoryOneCId, cancellationToken);

        if (pricesOneC.Count == 0)
            return;

        await DeletePricesMissingAsync(pricesOneC, rootCategoryOneCId, cancellationToken);
        await CreateOrUpsertPricesAsync(pricesOneC, rootCategoryOneCId, cancellationToken);

        logger.LogInformation("SyncOneCPricesJob finished for {Root}", rootCategoryOneCId);
    }

    private async Task DeletePricesMissingAsync(IReadOnlyList<OneCPriceDto> priceDtos, string productTypeIdOneC, CancellationToken cancellationToken)
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
            ).ToHashSet();

        var existing = await priceRepo.ListAsync(new PricesByProductIdsSpec(productIds, productTypeIdOneC), cancellationToken);

        var toDelete = existing
            .Where(p => !keepKeys.Contains((
                p.ProductTypeIdOneC,
                p.ProductId,
                p.PriceTypeId))
            ).ToList();

        if (toDelete.Count == 0) return;

        await priceRepo.DeleteRangeAsync(toDelete, cancellationToken);
    }

    private async Task CreateOrUpsertPricesAsync(IReadOnlyList<OneCPriceDto> priceDtos, string productTypeIdOneC, CancellationToken cancellationToken)
    {
        foreach (var item in priceDtos)
        {
            var productId = ProductId.From(item.ProductId);
            var priceTypeId = PriceTypeId.From(item.PriceTypeId);
            var priceValue = new Money(item.Price, item.Currency);

            var spec = new ProductPriceByProductAndTypeSpec(productId, priceTypeId);
            var existing = await priceRepo.FirstOrDefaultAsync(spec, cancellationToken);

            if (existing is null)
            {
                var prices = ProductPrice.Create(
                    productTypeIdOneC: productTypeIdOneC,
                    productId: productId,
                    priceTypeId: priceTypeId,
                    price: priceValue);

                await priceRepo.AddAsync(prices, cancellationToken);
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
