using Catalog.Contracts.SiteModels.Public;

namespace Catalog.Application.Features.SiteModels.Public.Get;

/// <summary>The model page by slug.</summary>
public sealed record GetPublicSiteModelQuery(string Lang, string Slug) : IQuery<Result<PublicSiteModelLookup>>;

/// <summary>The model, or where a former model slug or a product slug now lives (permanent redirect, M-11).</summary>
public sealed record PublicSiteModelLookup(PublicSiteModelDto? Model, PublicSiteModelAddressDto? Redirect);
