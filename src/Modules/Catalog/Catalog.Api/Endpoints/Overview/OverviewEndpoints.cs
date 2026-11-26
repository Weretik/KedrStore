using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.Overview.GetAdminCatalogOverview;
using Catalog.Api.Endpoints.Overview.GetAdminCatalogSeoReview;
namespace Catalog.Api.Endpoints.Overview;

/// <summary>Catalog problems shown on the admin «Огляд» and «SEO-огляд».</summary>
public static class OverviewEndpoints
{
    public static IEndpointRouteBuilder MapOverviewEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminCatalogOverview = endpoints.MapGroup("api/admin/catalog/overview")
            .WithTags("AdminCatalogOverview")
            .RequireAuthorization(PolicyNames.CanManageProducts);
        GetAdminCatalogOverviewEndpoint.Map(adminCatalogOverview);

        var adminCatalogSeoReview = endpoints.MapGroup("api/admin/catalog/seo-review")
            .WithTags("AdminCatalogSeoReview")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog);
        GetAdminCatalogSeoReviewEndpoint.Map(adminCatalogSeoReview);

        return endpoints;
    }
}
