using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.SiteModels.Public.Common;
using Catalog.Application.Slugs;
using Catalog.Contracts.Category;
using Catalog.Contracts.SiteModels.Public;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Public.GetProductAddress;

/// <summary>
/// A current or former product slug of an on-site product gives its model address. A product that left the site or was
/// deleted points to its category, as the product page does (spec catalog/005 R-018); an unknown slug is not found.
/// </summary>
public sealed class GetPublicProductModelAddressQueryHandler(
    PublicModelCatalogReader reader,
    ISlugService slugService,
    IReadCatalogDbContext db)
    : IQueryHandler<GetPublicProductModelAddressQuery, Result<CatalogLookup<PublicSiteModelAddressDto>>>
{
    public async ValueTask<Result<CatalogLookup<PublicSiteModelAddressDto>>> Handle(
        GetPublicProductModelAddressQuery query, CancellationToken cancellationToken)
    {
        var slug = query.Slug.Trim().ToLowerInvariant();
        var id = await slugService.ResolveAsync(CatalogEntityType.Product, slug, cancellationToken)
                 ?? await db.Products.IgnoreQueryFilters().AsNoTracking()
                     .Where(product => product.IsDeleted && product.ProductSlug == slug)
                     .Select(product => (int?)product.Id.Value)
                     .FirstOrDefaultAsync(cancellationToken);
        if (id is null)
            return Result.NotFound();

        var productId = ProductId.From(id.Value);
        var catalog = await reader.ReadAsync(cancellationToken);
        if (catalog.AddressOf(productId) is { } address)
            return Result.Success(CatalogLookup<PublicSiteModelAddressDto>.Found(address));

        var categoryId = await db.Products.IgnoreQueryFilters().AsNoTracking()
            .Where(product => product.Id == productId)
            .Select(product => product.CategoryId)
            .FirstAsync(cancellationToken);
        return Result.Success(CatalogLookup<PublicSiteModelAddressDto>.Gone(
            catalog.Categories.TryGetValue(categoryId, out var category) ? new CatalogRedirect(category.Slug) : null));
    }
}
