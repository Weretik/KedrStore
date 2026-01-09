// Licensed to KedrStore Development Team under MIT License.

using BuildingBlocks.Application.Integrations.OneC.Contracts;
using Catalog.Application.Features.Shared;
using Catalog.Application.Integrations.OneC.Mappers;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Application.Persistance;
using Catalog.Domain.Entities;
using Catalog.Domain.Enumerations;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCProductDetailsJob(
    IOneCClient oneC,
    ICatalogRepository<Product> productRepo,
    ICatalogRepository<ProductCategory> categoryRepo)
{
    public async Task RunAsync(string rootCategoryId, CancellationToken cancellationToken)
    {
        var productsOneC = await oneC.GetProductDetailsAsync(rootCategoryId, cancellationToken);

        if (productsOneC.Count == 0)
            return;

        var rows = await categoryRepo.ListAsync(new CategoryIdSlugMapSpec(), cancellationToken);
        var slugDictionary = rows.ToDictionary(x => x.Slug, x => x.Id.Value);

        var products = CatalogMapper.MapProduct(productsOneC, slugDictionary);

        await DeleteMissingAsync(products, cancellationToken);
        await CreateOrUpsertProductsAsync(products, cancellationToken);
    }
    private async Task DeleteMissingAsync(IReadOnlyList<ProductDto> productDtos, CancellationToken cancellationToken)
    {
        var importProductsIds = productDtos
            .Select(c => ProductId.From(c.Id))
            .Distinct()
            .ToArray();

        var spec  = new ProductsByIdsSpec(importProductsIds, true);
        await productRepo.DeleteRangeAsync(spec, cancellationToken);
    }

    private async Task CreateOrUpsertProductsAsync(IReadOnlyList<ProductDto> productDtos, CancellationToken cancellationToken)
    {

        foreach (var item in productDtos)
        {
            var productId = ProductId.From(item.Id);
            var existing = await productRepo.GetByIdAsync(productId, cancellationToken);
            if (existing is null)
            {
                var productList = Product.Create(
                    id: productId,
                    name: item.Name,
                    categoryId: ProductCategoryId.From(item.CategoryId),
                    photo: item.Photo,
                    scheme: item.Scheme,
                    stock: item.Stock,
                    qtyInPack: item.QuantityInPack,
                    isNew: item.IsNew,
                    isSale: item.IsSale,
                    createdDate: DateTimeOffset.UtcNow
                );
                await productRepo.AddAsync(productList, cancellationToken);
            }
            else
            {
                existing.Update(
                    name: item.Name,
                    categoryId: ProductCategoryId.From(item.CategoryId),
                    photo: item.Photo,
                    scheme: item.Scheme,
                    qtyInPack: item.QuantityInPack,
                    updatedDate: DateTimeOffset.UtcNow);

                if (item.IsNew) existing.MarkAsNew();
                else existing.RemoveNew();

                if(item.IsSale) existing.MarkAsSale();
                else existing.RemoveSale();
            }
        }
        await categoryRepo.SaveChangesAsync(cancellationToken);
    }
}
