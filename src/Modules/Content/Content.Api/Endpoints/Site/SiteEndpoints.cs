using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Site.RefreshSite;
namespace Content.Api.Endpoints.Site;

/// <summary>«Оновити сайт зараз»: asks the public site to drop its cached content and catalog.</summary>
public static class SiteEndpoints
{
    public static IEndpointRouteBuilder MapSiteEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminSiteRefresh = endpoints.MapGroup("api/admin/content/site-refresh")
            .WithTags("AdminSiteRefresh")
            .RequireAuthorization(PolicyNames.CanManageContent);
        RefreshSiteEndpoint.Map(adminSiteRefresh);

        return endpoints;
    }
}
