using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Projections;
using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Mappers;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Application.Integrations.OneC.Sync;
using Catalog.Application.Slugs;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCProductDetailsJob(
    IOneCClient oneC,
    ICatalogRepository<Product> productRepo,
    ICatalogRepository<ProductCategory> categoryRepo,
    ISlugService slugService,
    ProductTranslationImporter translationImporter,
    IOptionsSnapshot<RootCategoryId> rootCategoryOptions,
    IProductListProjectionRebuilder productListProjectionRebuilder,
    IProductPhotoCheckStore productPhotoCheckStore,
    ILogger<SyncOneCProductDetailsJob> logger)
{
    private const string Phase = "Product details import";

    public async Task RunAsync(
        string rootCategoryId,
        CancellationToken cancellationToken,
        bool rebuildProjection = true)
    {
        var fetchTimer = Stopwatch.StartNew();
        var productsOneC = await oneC.GetProductDetailsAsync(rootCategoryId, cancellationToken);
        logger.LogInformation(
            "Received {Count} products from 1C for {Root} in {ElapsedMs} ms.",
            productsOneC.Count, rootCategoryId, fetchTimer.ElapsedMilliseconds);

        if (productsOneC.Count == 0)
            return;

        var isCosmosRoot = string.Equals(
            rootCategoryId,
            rootCategoryOptions.Value.CosmosRootCategoryId,
            StringComparison.Ordinal);

        CategoryNameMap? categoryNames = null;
        int? fallbackCategoryId = null;
        if (isCosmosRoot)
        {
            fallbackCategoryId = rootCategoryOptions.Value.CosmosCategoryId;
        }
        else
        {
            var rows = await categoryRepo.ListAsync(new ActiveCategoryNamesByRootSpec(rootCategoryId), cancellationToken);
            categoryNames = new CategoryNameMap(rows);
        }

        var products = CatalogMapper.MapProduct(productsOneC, categoryNames, rootCategoryId, fallbackCategoryId);
        categoryNames?.LogSummary(logger, rootCategoryId);

        if (products.Count < productsOneC.Count)
        {
            logger.LogWarning(
                "OneC returned duplicate product IDs for root {Root}. Duplicates were ignored. Received: {ReceivedCount}; distinct: {DistinctCount}.",
                rootCategoryId,
                productsOneC.Count,
                products.Count);
        }

        if (isCosmosRoot)
            products = await ExcludeProductsOwnedByOtherRootsAsync(products, rootCategoryId, cancellationToken);

        if (products.Count == 0)
        {
            logger.LogWarning(
                "All mapped Cosmos products are already owned by another root. Catalog data was not changed.");
            return;
        }

        var counters = new ImportCounters { Received = products.Count };
        var syncedProductIds = await ApplyAsync(products, rootCategoryId, counters, cancellationToken);
        counters.Log(logger, Phase, rootCategoryId);

        await productPhotoCheckStore.ReconcileAsync(
            products
                .Select(product => new ProductPhotoCandidate(ProductId.From(product.Id), product.Photo))
                .ToArray(),
            cancellationToken);

        await translationImporter.ImportAsync(syncedProductIds, cancellationToken);

        if (rebuildProjection)
            await productListProjectionRebuilder.RebuildAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ProductRowOneCDto>> ExcludeProductsOwnedByOtherRootsAsync(
        IReadOnlyList<ProductRowOneCDto> products,
        string cosmosRootId,
        CancellationToken cancellationToken)
    {
        var productIds = products.Select(product => ProductId.From(product.Id)).ToArray();
        var conflicts = await productRepo.ListAsync(
            new ProductsByIdsFromOtherRootsSpec(productIds, cosmosRootId),
            cancellationToken);
        var conflictingIds = conflicts.Select(product => product.Id.Value).ToHashSet();

        if (conflictingIds.Count == 0)
            return products;

        logger.LogWarning(
            "Skipping {ConflictCount} Cosmos products because their IDs are already owned by another root.",
            conflictingIds.Count);

        return products
            .Where(product => !conflictingIds.Contains(product.Id))
            .ToList();
    }

    /// <summary>Creates, updates, restores, and soft-deletes the products of the root in one save.</summary>
    private async Task<IReadOnlyCollection<ProductId>> ApplyAsync(
        IReadOnlyList<ProductRowOneCDto> productDtos,
        string rootCategoryId,
        ImportCounters counters,
        CancellationToken cancellationToken)
    {
        var existingById = (await productRepo.ListAsync(new ProductsByRootSpec(rootCategoryId), cancellationToken))
            .ToDictionary(product => product.Id.Value);
        var now = DateTimeOffset.UtcNow;
        var created = new List<Product>();
        var synced = new List<ProductId>(productDtos.Count);
        SlugAllocator? slugs = null;

        foreach (var item in productDtos)
        {
            if (existingById.TryGetValue(item.Id, out var existing))
            {
                var restored = existing.Restore(now);
                var categoryId = item.CategoryId is { } resolved ? ProductCategoryId.From(resolved) : existing.CategoryId;
                var updated = existing.UpdateFromOneC(item.Name, categoryId, item.Photo, item.Scheme,
                    item.QuantityInPack, item.ExportToSite, item.IsNew, item.IsSale, now);

                if (restored) counters.Restored++;
                else if (updated) counters.Updated++;
                else counters.Unchanged++;

                synced.Add(existing.Id);
                continue;
            }

            if (item.CategoryId is not { } newCategoryId)
            {
                counters.Skipped++;
                continue;
            }

            slugs ??= await slugService.LoadAllocatorAsync(CatalogEntityType.Product, cancellationToken);
            var product = Product.Create(
                id: ProductId.From(item.Id),
                productTypeIdOneC: item.ProductTypeIdOneC,
                name: item.Name,
                productSlug: slugs.Allocate(item.Name),
                categoryId: ProductCategoryId.From(newCategoryId),
                photo: item.Photo,
                scheme: item.Scheme,
                createdDate: now,
                stock: item.Stock,
                qtyInPack: item.QuantityInPack,
                isNew: item.IsNew,
                isSale: item.IsSale,
                exportToSite: item.ExportToSite);
            created.Add(product);
            synced.Add(product.Id);
            counters.Created++;
        }

        MarkMissingDeleted(existingById.Values, productDtos, rootCategoryId, counters, now);

        if (created.Count > 0)
            await productRepo.AddRangeAsync(created, cancellationToken);

        await productRepo.SaveChangesAsync(cancellationToken);
        return synced;
    }

    private void MarkMissingDeleted(
        IEnumerable<Product> existing,
        IReadOnlyList<ProductRowOneCDto> productDtos,
        string rootCategoryId,
        ImportCounters counters,
        DateTimeOffset now)
    {
        var receivedIds = productDtos.Select(product => product.Id).ToHashSet();
        var active = existing.Where(product => !product.IsDeleted).ToList();
        var missing = active.Where(product => !receivedIds.Contains(product.Id.Value)).ToList();

        if (!MassDeletionGuard.Allows(active.Count, missing.Count))
        {
            counters.GuardBlocked = missing.Count;
            logger.LogError(
                "Mass deletion guard blocked soft-deleting {MissingCount} of {ActiveCount} active products for {Root}. Nothing was deleted.",
                missing.Count, active.Count, rootCategoryId);
            return;
        }

        foreach (var product in missing)
        {
            if (product.MarkDeleted(now))
                counters.Deleted++;
        }
    }
}
