// Licensed to KedrStore Development Team under MIT License.

using BuildingBlocks.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.Mappers;
using Catalog.Application.Persistance;
using Catalog.Domain.Entities;
using Catalog.Domain.Enumerations;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCProductDetailsJob(
    IOneCClient oneC,
    ICatalogRepository<Product> productRepo)
{
    public async Task RunAsync(string rootCategoryId, CancellationToken cancellationToken)
    {

        var products = await oneC.GetProductDetailsAsync(rootCategoryId, cancellationToken);

        if (products.Count == 0)
            return;

        var parsed = CatalogMapper.MapCatalog(products, );
    }
}
