using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Wholesale;

/// <summary>
/// Admin editing of the site page «Співпраця» in both languages. The texts of the site page «Співпраця».
/// </summary>
public static class WholesaleEndpoints
{
    public static IEndpointRouteBuilder MapWholesaleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminWholesalePage = endpoints.MapGroup("api/admin/content/wholesale")
            .WithTags("AdminWholesalePage")
            .RequireAuthorization(PolicyNames.CanManageContent);
        GetAdminWholesalePageEndpoint.Map(adminWholesalePage);
        UpdateAdminWholesalePageEndpoint.Map(adminWholesalePage);

        var wholesalePage = endpoints.MapGroup("api/content")
            .WithTags("WholesalePage")
            .AllowAnonymous();
        GetPublicWholesalePageEndpoint.Map(wholesalePage);

        return endpoints;
    }
}
