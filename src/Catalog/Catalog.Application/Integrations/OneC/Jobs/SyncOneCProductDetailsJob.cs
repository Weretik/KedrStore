using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Mappers;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCProductDetailsJob(
    IOneCClient oneC,
    ICatalogRepository<Product> productRepo,
    ICatalogRepository<ProductCategory> categoryRepo,
    ILogger<SyncOneCPricesJob> logger)
{
    public async Task RunAsync(string rootCategoryId, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DEBUG_LOG] SyncOneCProductDetailsJob started for {Root}", rootCategoryId);

        var productsOneC = await oneC.GetProductDetailsAsync(rootCategoryId, cancellationToken);
        logger.LogInformation("[DEBUG_LOG] Received {Count} products from 1C for root {Root}", productsOneC.Count, rootCategoryId);

        if (productsOneC.Count == 0)
            return;

        var rows = await categoryRepo.ListAsync(new CategoryIdSlugMapSpec(), cancellationToken);
        var categoryNameDictionary = rows.ToDictionary(x => x.CategoryName, x => x.Id.Value);

        var products = CatalogMapper.MapProduct(productsOneC, categoryNameDictionary, rootCategoryId);

        await DeleteMissingAsync(products, rootCategoryId, cancellationToken);
        await CreateOrUpsertProductsAsync(products, rootCategoryId, cancellationToken);

        logger.LogInformation("SyncOneCProductDetailsJob finished for {Root}", rootCategoryId);
    }
    private async Task DeleteMissingAsync(IReadOnlyList<ProductRowOneCDto> productDtos, string rootCategoryOneCId, CancellationToken cancellationToken)
    {
        var importProductsIds = productDtos
            .Select(c => ProductId.From(c.Id))
            .Distinct()
            .ToArray();

        var spec  = new ProductsByIdsSpec(importProductsIds, rootCategoryOneCId,true);
        await productRepo.DeleteRangeAsync(spec, cancellationToken);
    }

    private async Task CreateOrUpsertProductsAsync(IReadOnlyList<ProductRowOneCDto> productDtos, string rootCategoryId, CancellationToken cancellationToken)
    {
        // FIX: Получаем все ID из этого пака данных одним запросом, чтобы избежать конфликтов и ускорить работу
        var productIdsInBatch = productDtos.Select(x => ProductId.From(x.Id)).ToList();
        var existingProducts = await productRepo.ListAsync(new ProductsByIdsSpec(productIdsInBatch, rootCategoryId), cancellationToken);
        var existingDict = existingProducts.ToDictionary(x => x.Id, x => x);

        foreach (var item in productDtos)
        {
            var productId = ProductId.From(item.Id);

            if (!existingDict.TryGetValue(productId, out var existing))
            {
                var product = Product.Create(
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
                await productRepo.AddAsync(product, cancellationToken);
                // Добавляем в словарь, чтобы избежать дублей внутри одного цикла, если 1С прислала повтор
                existingDict[productId] = product;
                logger.LogInformation("[DEBUG_LOG] Added new product: {Name} (ID: {Id})", product.Name, product.Id);
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

                if (item.IsSale) existing.MarkAsSale();
                else existing.RemoveSale();
            }
        }
        await productRepo.SaveChangesAsync(cancellationToken);
    }
}
