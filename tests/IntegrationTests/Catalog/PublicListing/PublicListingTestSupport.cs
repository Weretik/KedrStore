using Catalog.Application.Features.Products.GetBySlug;
using Catalog.Application.Features.Products.GetFacets;
using Catalog.Application.Features.Products.GetPublicList;
using Catalog.Application.Features.Products.PublicListing;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Application.Slugs;
using Catalog.Infrastructure.DataBase;
using IntegrationTests.Catalog.ProductMedia;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Catalog.PublicListing;

/// <summary>Public list, facets, and product variants over an in-memory catalog.</summary>
internal static class PublicListingTestSupport
{
    public static readonly IOptions<RootCategoryId> Roots = Options.Create(new RootCategoryId
    {
        DoorsRootCategoryId = "000007226",
        HardwareRootCategoryId = "000005513",
        CosmosRootCategoryId = "Космос",
        CosmosCategoryId = 920000
    });

    public static VariantGroupReader Groups(CatalogDbContext db, FakeProductMediaLibrary? media = null)
        => new(db, media ?? new FakeProductMediaLibrary());

    public static GetPublicProductListQueryHandler ListHandler(CatalogDbContext db, FakeProductMediaLibrary? media = null)
        => new(db, new PublicProductQuery(db, new SlugService(db), Roots), new AttributeFilters(db), Groups(db, media));

    public static GetCategoryFacetsQueryHandler FacetsHandler(CatalogDbContext db, FakeProductMediaLibrary? media = null)
        => new(db, new PublicProductQuery(db, new SlugService(db), Roots), new AttributeFilters(db), Groups(db, media));

    public static ProductVariantsReader VariantsReader(CatalogDbContext db, FakeProductMediaLibrary? media = null)
        => new(db, Groups(db, media));
}
