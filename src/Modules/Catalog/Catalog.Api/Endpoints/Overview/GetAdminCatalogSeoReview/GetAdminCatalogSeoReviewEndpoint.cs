using Catalog.Application.Features.Overview.Queries.GetCatalogSeoReview;
using Catalog.Contracts.Overview;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Overview.GetAdminCatalogSeoReview;

internal static class GetAdminCatalogSeoReviewEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminCatalogSeoReview")
            .WithSummary("Category and model pages whose search texts need work")
            .WithDescription("Requires `CanManageSiteCatalog` (Seo or Admin). Spec platform/010: categories shown on the site and open to search engines, and accepted models in them, per language; only pages with an issue are listed. Read-only.")
            .Produces<IReadOnlyList<SeoReviewItemResponse>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCatalogSeoReviewQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}
