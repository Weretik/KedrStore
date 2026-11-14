using BuildingBlocks.Application.Snapshots;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Application.Shared;
using Catalog.Contracts.MobileSync;
using Catalog.Contracts.Pricing;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;

namespace Catalog.Application.Features.MobileSync.Queries.GetCatalogSnapshot;

/// <summary>
/// Builds the catalog snapshot: every product the sales catalog offers (the query filters already leave out deleted
/// products and categories) with all its UAH prices, Ukrainian name, photos and scheme. Rows are ordered by ID so the
/// same data always gives the same <see cref="CatalogSnapshotResponse.Version"/>.
/// </summary>
public sealed class GetCatalogSnapshotQueryHandler(
    IReadCatalogDbContext catalogDbContext,
    IOptionsSnapshot<RootCategoryId> rootOptions,
    IOptionsSnapshot<CatalogPricingOptions> pricingOptions,
    TimeProvider timeProvider)
    : IQueryHandler<GetCatalogSnapshotQuery, Result<CatalogSnapshotResponse>>
{
    private const string PriceCurrency = "UAH";

    public async ValueTask<Result<CatalogSnapshotResponse>> Handle(GetCatalogSnapshotQuery query,
        CancellationToken cancellationToken)
    {
        var hardwareRoot = rootOptions.Value.HardwareRootCategoryId;

        var priceTypes = await catalogDbContext.PriceTypes.AsNoTracking()
            .OrderBy(type => type.Id)
            .Select(type => new CatalogSnapshotPriceType(type.Id.Value, type.PriceTypeName))
            .ToListAsync(cancellationToken);

        var categories = (await catalogDbContext.Categories.AsNoTracking()
                .Select(category => new { category.Id, category.ParentId, category.Name, category.SortOrder })
                .ToListAsync(cancellationToken))
            .OrderBy(category => category.Id.Value)
            .Select(category => new CatalogSnapshotCategory(
                category.Id.Value, category.ParentId?.Value, category.Name, category.SortOrder))
            .ToList();

        var names = (await catalogDbContext.ProductTranslations.AsNoTracking()
                .Where(translation => translation.Language == CatalogLanguage.Ukrainian)
                .Select(translation => new { translation.ProductId, translation.Name })
                .ToListAsync(cancellationToken))
            .GroupBy(translation => translation.ProductId.Value)
            .ToDictionary(group => group.Key, group => group.First().Name);

        var prices = (await catalogDbContext.ProductPrices.AsNoTracking()
                .Where(price => price.CurrencyIso == PriceCurrency)
                .Select(price => new { price.ProductId, price.PriceTypeId, price.Amount })
                .ToListAsync(cancellationToken))
            .ToLookup(price => price.ProductId.Value);

        var products = (await catalogDbContext.Products.AsNoTracking().ToListAsync(cancellationToken))
            .OrderBy(product => product.Id.Value)
            .Select(product => new CatalogSnapshotProduct(
                product.Id.Value,
                product.ProductSlug,
                names.GetValueOrDefault(product.Id.Value, product.Name),
                product.CategoryId.Value,
                product.Stock,
                product.ProductTypeIdOneC == hardwareRoot ? product.Stock > 2 : product.Stock > 0,
                product.QuantityInPack,
                product.IsSale,
                product.IsNew,
                product.MediaLinks
                    .Where(link => link.Kind == ProductMediaKind.Photo)
                    .OrderBy(link => link.Position)
                    .Select(link => link.Url)
                    .ToList(),
                product.MediaLinks.FirstOrDefault(link => link.Kind == ProductMediaKind.Scheme)?.Url,
                prices[product.Id.Value]
                    .GroupBy(price => price.PriceTypeId.Value)
                    .OrderBy(group => group.Key)
                    .Select(group => new CatalogSnapshotPrice(group.Key, group.First().Amount))
                    .ToList()))
            .ToList();

        var retailPriceTypeId = pricingOptions.Value.RetailPriceTypeId;
        var version = SnapshotVersion.Compute(new { retailPriceTypeId, priceTypes, categories, products });

        return Result.Success(new CatalogSnapshotResponse(
            version, timeProvider.GetUtcNow(), retailPriceTypeId, priceTypes, categories, products));
    }
}
