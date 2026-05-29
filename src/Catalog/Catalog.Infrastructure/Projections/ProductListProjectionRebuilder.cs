using Catalog.Application.Contracts.Projections;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Contracts.Pricing;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Projections;

internal sealed class ProductListProjectionRebuilder(
    CatalogDbContext catalogDbContext,
    IOptionsSnapshot<CatalogPricingOptions> pricingOptions,
    IOptionsSnapshot<RootCategoryId> rootCategoryOptions,
    ILogger<ProductListProjectionRebuilder> logger)
    : IProductListProjectionRebuilder
{
    private const string RuLanguage = "ru";

    public async Task RebuildAsync(CancellationToken cancellationToken = default)
    {
        var retailPriceTypeId = PriceTypeId.From(pricingOptions.Value.RetailPriceTypeId);
        var hardwareRootCategoryId = rootCategoryOptions.Value.HardwareRootCategoryId;

        logger.LogInformation("Product list projection rebuild started.");

        var products = await catalogDbContext.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var productIds = products
            .Select(product => product.Id)
            .ToArray();

        var retailPrices = await catalogDbContext.ProductPrices
            .AsNoTracking()
            .Where(price => price.PriceTypeId == retailPriceTypeId && productIds.Contains(price.ProductId))
            .ToDictionaryAsync(price => price.ProductId, price => price.Amount, cancellationToken);

        var categorySlugs = await catalogDbContext.Categories
            .AsNoTracking()
            .ToDictionaryAsync(category => category.Id, category => category.Slug, cancellationToken);

        var ruTranslations = await catalogDbContext.ProductTranslations
            .AsNoTracking()
            .Where(translation => translation.Language == RuLanguage && productIds.Contains(translation.ProductId))
            .ToDictionaryAsync(translation => translation.ProductId, translation => translation.Name, cancellationToken);

        var projections = products
            .Select(product => ProductListProjection.Create(
                productId: product.Id,
                nameUk: product.Name,
                nameRu: ruTranslations.GetValueOrDefault(product.Id) ?? product.Name,
                productSlug: product.ProductSlug,
                photo: product.Photo,
                categoryId: product.CategoryId,
                categorySlug: categorySlugs.GetValueOrDefault(product.CategoryId) ?? product.CategoryId.Value.ToString(CultureInfo.InvariantCulture),
                inStock: product.ProductTypeIdOneC == hardwareRootCategoryId
                    ? product.Stock > 2
                    : product.Stock > 0,
                isSale: product.IsSale,
                isNew: product.IsNew,
                retailPrice: retailPrices.GetValueOrDefault(product.Id)))
            .ToArray();

        await catalogDbContext.ProductListProjections.ExecuteDeleteAsync(cancellationToken);
        await catalogDbContext.ProductListProjections.AddRangeAsync(projections, cancellationToken);
        await catalogDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Product list projection rebuild finished. Products: {ProductsCount}, Projections: {ProjectionsCount}, RetailPrices: {RetailPricesCount}, RuTranslations: {RuTranslationsCount}, Categories: {CategoriesCount}.",
            products.Count,
            projections.Length,
            retailPrices.Count,
            ruTranslations.Count,
            categorySlugs.Count);
    }
}
