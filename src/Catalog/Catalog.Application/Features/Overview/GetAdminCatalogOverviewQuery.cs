using Catalog.Contracts.Overview;

namespace Catalog.Application.Features.Overview;

public sealed record GetAdminCatalogOverviewQuery : IQuery<Result<AdminCatalogOverviewResponse>>;
