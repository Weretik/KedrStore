using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.GetShowcase;

public sealed record GetSiteShowcaseQuery(GetSiteShowcaseRequest Request) : IQuery<Result<SiteShowcaseListDto>>
{
    public const int MaxPageSize = 200;
}
