using Catalog.Contracts.Category;
using Catalog.Contracts.SiteModels.Public;

namespace Catalog.Application.Features.SiteModels.Public.GetProductAddress;

/// <summary>Where the old product address now lives on the site: its model and the product's axis values (M-11).</summary>
public sealed record GetPublicProductModelAddressQuery(string Slug)
    : IQuery<Result<CatalogLookup<PublicSiteModelAddressDto>>>;
