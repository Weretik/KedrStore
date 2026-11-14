using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.Overview.GetAdminCatalogOverview;
namespace Catalog.Api.Endpoints.Overview;

/// <summary>Catalog problems shown on the admin «Огляд».</summary>
public static class OverviewEndpoints
{
    public static IEndpointRouteBuilder MapOverviewEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminCatalogOverview = endpoints.MapGroup("api/admin/catalog/overview")
            .WithTags("AdminCatalogOverview")
            .RequireAuthorization(PolicyNames.CanManageProducts);
        GetAdminCatalogOverviewEndpoint.Map(adminCatalogOverview);

        return endpoints;
    }
}
