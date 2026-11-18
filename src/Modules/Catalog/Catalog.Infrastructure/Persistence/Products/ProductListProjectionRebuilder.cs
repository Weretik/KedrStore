using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Contracts.Pricing;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;

namespace Catalog.Infrastructure.Persistence.Products;

internal sealed class ProductListProjectionRebuilder(
    IDbContextFactory<CatalogDbContext> catalogDbContextFactory,
    IOptionsSnapshot<CatalogPricingOptions> pricingOptions,
    IOptionsSnapshot<RootCategoryId> rootCategoryOptions,
    ILogger<ProductListProjectionRebuilder> logger)
    : IProductListProjectionRebuilder
{
    private const string RuLanguage = "ru";

    /// <summary>Advisory lock key shared by every product list rebuild (any constant unique in this database).</summary>
    private const long RebuildLockKey = 7_001_001;

    public async Task<int> RefreshPhotosAsync(CancellationToken cancellationToken = default)
    {
        await using var catalogDbContext = await catalogDbContextFactory.CreateDbContextAsync(cancellationToken);
        return await catalogDbContext.Database.ExecuteSqlRawAsync(
            """
            UPDATE catalog."ProductListProjections" AS projection
            SET "Photo" = product."Photo"
            FROM catalog."Products" AS product
            WHERE product."Id" = projection."ProductId" AND projection."Photo" <> product."Photo"
            """,
            cancellationToken);
    }

    /// <summary>
    /// Brings the list in line with the catalog in one transaction: changed rows are updated, new rows added and gone
    /// rows removed, so readers never see an empty or half-built list. A transaction-scoped advisory lock makes a second
    /// rebuild wait for the first (database rules 2.4, 3.2, 4).
    /// </summary>
    public async Task RebuildAsync(CancellationToken cancellationToken = default)
    {
        await using var catalogDbContext = await catalogDbContextFactory.CreateDbContextAsync(cancellationToken);
        var strategy = catalogDbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            catalogDbContext.ChangeTracker.Clear();
            await using var transaction = await catalogDbContext.Database.BeginTransactionAsync(cancellationToken);
            await catalogDbContext.Database.ExecuteSqlAsync(
                $"SELECT pg_advisory_xact_lock({RebuildLockKey})", cancellationToken);
            await RebuildInTransactionAsync(catalogDbContext, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private async Task RebuildInTransactionAsync(CatalogDbContext catalogDbContext, CancellationToken cancellationToken)
    {
        var retailPriceTypeId = PriceTypeId.Create(pricingOptions.Value.RetailPriceTypeId);
        var hardwareRootCategoryId = rootCategoryOptions.Value.HardwareRootCategoryId;

        logger.LogInformation("Product list projection rebuild started.");

        var products = await catalogDbContext.Products
            .AsNoTracking()
            .Select(product => new
            {
                product.Id,
                product.Name,
                product.ProductSlug,
                product.Photo,
                product.CategoryId,
                product.ProductTypeIdOneC,
                product.Stock,
                product.IsSale,
                product.IsNew,
                product.ExportToSite
            })
            .ToListAsync(cancellationToken);

        var retailPrices = await catalogDbContext.ProductPrices
            .AsNoTracking()
            .Where(price => price.PriceTypeId == retailPriceTypeId)
            .ToDictionaryAsync(price => price.ProductId, price => price.Amount, cancellationToken);

        var categorySlugs = await catalogDbContext.Categories
            .AsNoTracking()
            .ToDictionaryAsync(category => category.Id, category => category.Slug, cancellationToken);

        var ruTranslations = await catalogDbContext.ProductTranslations
            .AsNoTracking()
            .Where(translation => translation.Language == RuLanguage)
            .ToDictionaryAsync(translation => translation.ProductId, translation => translation.Name, cancellationToken);

        // Products of soft-deleted categories stay out of every list (R-005); the filter hides those categories.
        var desired = products
            .Where(product => categorySlugs.ContainsKey(product.CategoryId))
            .Select(product => ProductListProjection.Create(
                productId: product.Id,
                nameUk: product.Name,
                nameRu: ruTranslations.GetValueOrDefault(product.Id) ?? product.Name,
                productSlug: product.ProductSlug,
                photo: product.Photo,
                categoryId: product.CategoryId,
                categorySlug: categorySlugs[product.CategoryId],
                inStock: product.ProductTypeIdOneC == hardwareRootCategoryId
                    ? product.Stock > 2
                    : product.Stock > 0,
                isSale: product.IsSale,
                isNew: product.IsNew,
                exportToSite: product.ExportToSite,
                retailPrice: retailPrices.GetValueOrDefault(product.Id)))
            .ToDictionary(projection => projection.ProductId);

        // Tracked on purpose: this is the write side of the rebuild, and only changed rows are updated.
        var existing = await catalogDbContext.ProductListProjections.ToListAsync(cancellationToken);

        var (changed, removed) = (0, 0);
        foreach (var row in existing)
        {
            if (!desired.Remove(row.ProductId, out var source))
            {
                catalogDbContext.ProductListProjections.Remove(row);
                removed++;
            }
            else if (row.SyncWith(source))
                changed++;
        }

        catalogDbContext.ProductListProjections.AddRange(desired.Values);
        await catalogDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Product list projection rebuild finished. Products: {ProductsCount}, Added: {AddedCount}, Changed: {ChangedCount}, Removed: {RemovedCount}, RetailPrices: {RetailPricesCount}, RuTranslations: {RuTranslationsCount}, Categories: {CategoriesCount}.",
            products.Count,
            desired.Count,
            changed,
            removed,
            retailPrices.Count,
            ruTranslations.Count,
            categorySlugs.Count);
    }
}
