using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Overview.GetAdminContentOverview;
using Content.Api.Endpoints.Overview.GetAdminContentSeoReview;
namespace Content.Api.Endpoints.Overview;

/// <summary>Site content problems shown on the admin «Огляд» and «SEO-огляд».</summary>
public static class OverviewEndpoints
{
    public static IEndpointRouteBuilder MapOverviewEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminContentOverview = endpoints.MapGroup("api/admin/content/overview")
            .WithTags("AdminContentOverview")
            .RequireAuthorization(PolicyNames.CanManageContent);
        GetAdminContentOverviewEndpoint.Map(adminContentOverview);

        var adminContentSeoReview = endpoints.MapGroup("api/admin/content/seo-review")
            .WithTags("AdminContentSeoReview")
            .RequireAuthorization(PolicyNames.CanManageContent);
        GetAdminContentSeoReviewEndpoint.Map(adminContentSeoReview);

        return endpoints;
    }
}
