using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.GetHealth;

public sealed record GetSiteModelHealthQuery : IQuery<Result<SiteModelHealthDto>>;
