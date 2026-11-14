using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Home.DeleteAdminHomeFaqItem;

using Content.Api.Endpoints.Home.GetAdminHomeBlocks;
using Content.Api.Endpoints.Home.GetPublicHomeBlocks;
using Content.Api.Endpoints.Home.DeleteAdminHomeBrand;
using Content.Api.Endpoints.Home.UpdateAdminHomeBrand;
using Content.Api.Endpoints.Home.ReorderAdminHomeBrands;
using Content.Api.Endpoints.Home.CreateAdminHomeBrand;
using Content.Api.Endpoints.Home.CreateAdminHomeFaqItem;
using Content.Api.Endpoints.Home.ReorderAdminHomeFaq;
using Content.Api.Endpoints.Home.UpdateAdminHomeFaqItem;

namespace Content.Api.Endpoints.Home;

/// <summary>
/// Admin editing of the home page FAQ and brands. Every change answers with both lists in site order, so the page
/// needs no second request. The home page FAQ and brands the site shows.
/// </summary>
public static class HomeEndpoints
{
    public static IEndpointRouteBuilder MapHomeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminHomeBlocks = endpoints.MapGroup("api/admin/content/home")
            .WithTags("AdminHomeBlocks")
            .RequireAuthorization(PolicyNames.CanManageContent);
        GetAdminHomeBlocksEndpoint.Map(adminHomeBlocks);
        CreateAdminHomeFaqItemEndpoint.Map(adminHomeBlocks);
        UpdateAdminHomeFaqItemEndpoint.Map(adminHomeBlocks);
        DeleteAdminHomeFaqItemEndpoint.Map(adminHomeBlocks);
        ReorderAdminHomeFaqEndpoint.Map(adminHomeBlocks);
        CreateAdminHomeBrandEndpoint.Map(adminHomeBlocks);
        UpdateAdminHomeBrandEndpoint.Map(adminHomeBlocks);
        DeleteAdminHomeBrandEndpoint.Map(adminHomeBlocks);
        ReorderAdminHomeBrandsEndpoint.Map(adminHomeBlocks);

        var homeBlocks = endpoints.MapGroup("api/content")
            .WithTags("HomeBlocks")
            .AllowAnonymous();
        GetPublicHomeBlocksEndpoint.Map(homeBlocks);

        return endpoints;
    }
}
