using Catalog.Contracts.SiteModels.Public;
namespace Catalog.Api.Endpoints.SiteModels.Shared;

/// <summary>The query of the public model cards; a missing value takes the default of <see cref="GetPublicSiteModelsRequest"/>.</summary>
public sealed record GetPublicSiteModelsParameters
{
    public string[]? Filter { get; init; }
    public string? Search { get; init; }
    public bool? InStock { get; init; }
    public bool? IsSale { get; init; }
    public bool? IsNew { get; init; }
    public decimal? PriceFrom { get; init; }
    public decimal? PriceTo { get; init; }
    public string? Sort { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }

    public GetPublicSiteModelsRequest ToRequest()
    {
        var defaults = new GetPublicSiteModelsRequest();
        return defaults with
        {
            Filter = Filter, Search = Search, InStock = InStock, IsSale = IsSale, IsNew = IsNew,
            PriceFrom = PriceFrom, PriceTo = PriceTo, Sort = Sort,
            Page = Page ?? defaults.Page, PageSize = PageSize ?? defaults.PageSize
        };
    }
}
