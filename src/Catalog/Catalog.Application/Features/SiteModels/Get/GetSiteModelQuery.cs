using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Get;

public sealed record GetSiteModelQuery(int ModelId) : IQuery<Result<SiteModelDto>>;
