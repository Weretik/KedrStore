using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.SiteSeo.ListAdminPageSeo;

using Content.Api.Endpoints.SiteSeo.ListPublicPageSeo;
using Content.Api.Endpoints.SiteSeo.UpdateAdminPageSeo;

namespace Content.Api.Endpoints.SiteSeo;

/// <summary>
/// Admin editing of the search titles and descriptions of the site pages. Editor-set search titles and descriptions
/// of the site pages; the site keeps its own for the rest.
/// </summary>
public static class SiteSeoEndpoints
{
    public static IEndpointRouteBuilder MapSiteSeoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminPageSeo = endpoints.MapGroup("api/admin/content/page-seo")
            .WithTags("AdminPageSeo")
            .RequireAuthorization(PolicyNames.CanManageContent);
        ListAdminPageSeoEndpoint.Map(adminPageSeo);
        UpdateAdminPageSeoEndpoint.Map(adminPageSeo);

        var pageSeo = endpoints.MapGroup("api/content")
            .WithTags("PageSeo")
            .AllowAnonymous();
        ListPublicPageSeoEndpoint.Map(pageSeo);

        return endpoints;
    }
}
