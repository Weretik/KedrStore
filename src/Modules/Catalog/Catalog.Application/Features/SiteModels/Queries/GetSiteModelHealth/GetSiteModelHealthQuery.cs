using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Queries.GetSiteModelHealth;

public sealed record GetSiteModelHealthQuery : IQuery<Result<SiteModelHealthDto>>;
