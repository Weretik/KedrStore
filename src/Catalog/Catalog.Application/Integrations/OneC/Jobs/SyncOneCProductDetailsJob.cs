using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Mappers;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Application.Persistance;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCProductDetailsJob(
    IOneCClient oneC,
    ICatalogRepository<Product> productRepo,
    ICatalogRepository<ProductCategory> categoryRepo,
    ILogger<SyncOneCPricesJob> logger)
{
    [DisableConcurrentExecution(60 * 60 * 2)]
    public async Task RunAsync(string rootCategoryId, CancellationToken cancellationToken)
    {
        logger.LogInformation("SyncOneCProductDetailsJob started for {Root}", rootCategoryId);

        var productsOneC = await oneC.GetProductDetailsAsync(rootCategoryId, cancellationToken);

        if (productsOneC.Count == 0)
            return;

        var rows = await categoryRepo.ListAsync(new CategoryIdSlugMapSpec(), cancellationToken);
        var slugDictionary = rows.ToDictionary(x => x.Slug, x => x.Id.Value);

        var products = CatalogMapper.MapProduct(productsOneC, slugDictionary, rootCategoryId);

        await DeleteMissingAsync(products, rootCategoryId, cancellationToken);
        await CreateOrUpsertProductsAsync(products, cancellationToken);

        logger.LogInformation("SyncOneCProductDetailsJob finished for {Root}", rootCategoryId);
    }
    private async Task DeleteMissingAsync(IReadOnlyList<ProductDto> productDtos, string rootCategoryOneCId, CancellationToken cancellationToken)
    {
        var importProductsIds = productDtos
            .Select(c => ProductId.From(c.Id))
            .Distinct()
            .ToArray();

        var spec  = new ProductsByIdsSpec(importProductsIds, rootCategoryOneCId,true);
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
                    productTypeIdOneC: item.ProductTypeIdOneC,
                    name: item.Name,
                    productSlug: item.ProducSlug,
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
                    productSlug: item.ProducSlug,
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
        await productRepo.SaveChangesAsync(cancellationToken);
    }
}
