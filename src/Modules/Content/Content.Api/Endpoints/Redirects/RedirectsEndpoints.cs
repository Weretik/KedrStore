using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Content.Api.Endpoints.Redirects.CreateAdminSiteRedirect;
using Content.Api.Endpoints.Redirects.DeleteAdminSiteRedirect;
using Content.Api.Endpoints.Redirects.ListAdminSiteRedirects;
using Content.Api.Endpoints.Redirects.ListPublicSiteRedirects;
using Content.Api.Endpoints.Redirects.UpdateAdminSiteRedirect;

namespace Content.Api.Endpoints.Redirects;

/// <summary>Site redirects (spec content/013): edited in the admin «Переадресації», read whole by the site.</summary>
public static class RedirectsEndpoints
{
    public static IEndpointRouteBuilder MapRedirectsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("api/admin/content/redirects")
            .WithTags("AdminSiteRedirects")
            .RequireAuthorization(PolicyNames.CanManageContent);
        ListAdminSiteRedirectsEndpoint.Map(admin);
        CreateAdminSiteRedirectEndpoint.Map(admin);
        UpdateAdminSiteRedirectEndpoint.Map(admin);
        DeleteAdminSiteRedirectEndpoint.Map(admin);

        var site = endpoints.MapGroup("api/content")
            .WithTags("SiteRedirects")
            .AllowAnonymous().RequireRateLimiting("PublicRead");
        ListPublicSiteRedirectsEndpoint.Map(site);

        return endpoints;
    }
}
