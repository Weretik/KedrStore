using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Queries.GetSiteShowcase;

public sealed record GetSiteShowcaseQuery(GetSiteShowcaseRequest Request) : IQuery<Result<SiteShowcaseListDto>>
{
    public const int MaxPageSize = 200;

    /// <summary>The status filter value for products in no model.</summary>
    public const string NoModelStatus = "none";
}
