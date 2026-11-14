using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Queries.GetSiteModel;

public sealed record GetSiteModelQuery(int ModelId) : IQuery<Result<SiteModelDto>>;
