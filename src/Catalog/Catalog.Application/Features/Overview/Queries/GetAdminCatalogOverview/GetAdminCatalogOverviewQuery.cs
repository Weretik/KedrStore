using Catalog.Contracts.Overview;

namespace Catalog.Application.Features.Overview.Queries.GetAdminCatalogOverview;

public sealed record GetAdminCatalogOverviewQuery : IQuery<Result<AdminCatalogOverviewResponse>>;
