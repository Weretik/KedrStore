using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Queries.GetSiteModels;

public sealed record GetSiteModelsQuery(GetSiteModelsRequest Request) : IQuery<Result<SiteModelListDto>>
{
    public const int MaxPageSize = 200;
}
