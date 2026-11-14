using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.SeoSettings.GetSiteSeoSettings;

using Content.Api.Endpoints.SeoSettings.GetAdminSiteSeoSettings;
using Content.Api.Endpoints.SeoSettings.UpdateAdminSiteSeoSettings;

namespace Content.Api.Endpoints.SeoSettings;

/// <summary>
/// «Налаштування SEO сайту»: analytics ids, search console verification codes, link-preview picture. Public SEO
/// codes and the link-preview picture that every site page carries.
/// </summary>
public static class SeoSettingsEndpoints
{
    public static IEndpointRouteBuilder MapSeoSettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminSiteSeoSettings = endpoints.MapGroup("api/admin/content/seo-settings")
            .WithTags("AdminSiteSeoSettings")
            .RequireAuthorization(PolicyNames.CanManageContent);
        GetAdminSiteSeoSettingsEndpoint.Map(adminSiteSeoSettings);
        UpdateAdminSiteSeoSettingsEndpoint.Map(adminSiteSeoSettings);

        var siteSeoSettings = endpoints.MapGroup("api/content/seo-settings")
            .WithTags("SiteSeoSettings")
            .AllowAnonymous();
        GetSiteSeoSettingsEndpoint.Map(siteSeoSettings);

        return endpoints;
    }
}
