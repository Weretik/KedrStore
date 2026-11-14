using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.SiteModels;

/// <summary>
/// Site models (spec catalog/007): the models the public site is built from, their axes, products, and texts.
/// Manager and Admin edit them; the model slug keeps its spec 006 route, where only Admin and Seo change it. The
/// public site's models (spec catalog/007 stage 5): category lists with filters, the search, the model page, and
/// the model address of an old product address.
/// </summary>
public static class SiteModelsEndpoints
{
    public static IEndpointRouteBuilder MapSiteModelsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminSiteModels = endpoints.MapGroup("api/admin/site-models")
            .WithTags("AdminSiteModels")
            .RequireAuthorization(PolicyNames.CanManageProducts);
        GetAdminSiteModelsEndpoint.Map(adminSiteModels);
        GetAdminSiteModelHealthEndpoint.Map(adminSiteModels);
        GetAdminSiteShowcaseEndpoint.Map(adminSiteModels);
        GetAdminSiteModelCategoriesEndpoint.Map(adminSiteModels);
        GetAdminSiteModelEndpoint.Map(adminSiteModels);
        CreateAdminSiteModelEndpoint.Map(adminSiteModels);
        RenameAdminSiteModelEndpoint.Map(adminSiteModels);
        AcceptAdminSiteModelsEndpoint.Map(adminSiteModels);
        SetAdminSiteModelAxesEndpoint.Map(adminSiteModels);
        AddAdminSiteModelProductsEndpoint.Map(adminSiteModels);
        RemoveAdminSiteModelProductEndpoint.Map(adminSiteModels);
        SetAdminSiteModelTextEndpoint.Map(adminSiteModels);
        FillAdminSiteModelFromNamesEndpoint.Map(adminSiteModels);

        var siteModels = endpoints.MapGroup("api/catalog/{lang}")
            .WithTags("SiteModels")
            .AllowAnonymous();
        GetPublicSiteModelsEndpoint.Map(siteModels);
        GetPublicCategorySiteModelsEndpoint.Map(siteModels);
        GetPublicSiteModelEndpoint.Map(siteModels);
        GetPublicProductModelAddressEndpoint.Map(siteModels);

        return endpoints;
    }
}
