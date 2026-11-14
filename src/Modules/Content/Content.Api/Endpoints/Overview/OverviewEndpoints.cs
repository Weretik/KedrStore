using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Overview;

/// <summary>Site content problems shown on the admin «Огляд».</summary>
public static class OverviewEndpoints
{
    public static IEndpointRouteBuilder MapOverviewEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminContentOverview = endpoints.MapGroup("api/admin/content/overview")
            .WithTags("AdminContentOverview")
            .RequireAuthorization(PolicyNames.CanManageContent);
        GetAdminContentOverviewEndpoint.Map(adminContentOverview);

        return endpoints;
    }
}
