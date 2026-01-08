using BuildingBlocks.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Application.Persistance;
using Catalog.Domain.Entities;
using Catalog.Domain.Enumerations;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;
/*
public sealed class SyncOneCPricesJob(
    IOneCClient oneC,
    ICatalogRepository<Product> productRepo)
{
    public async Task RunAsync(string rootCategoryId, CancellationToken cancellationToken)
    {
        var prices = await oneC.GetProductPricesAsync(rootCategoryId, cancellationToken);

        if (prices.Count == 0)
            return;

        var priceByProductId = prices
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToDictionary(
                x => ProductId.From(int.Parse(x.Id.TrimStart('0'))),
                x => x.Price
            );

        if (priceByProductId.Count == 0)
            return;

        var spec = new ProductsByIdsSpec(priceByProductId.Keys);
        var products = await productRepo.ListAsync(spec, cancellationToken);


        foreach (var product in products)
        {
            if (!priceByProductId.TryGetValue(product.Id, out var price))
                continue;

            product.UpsertPrice(PriceType.Retail, money);
        }

        await productRepo.SaveChangesAsync(cancellationToken);
    }
}
*/
