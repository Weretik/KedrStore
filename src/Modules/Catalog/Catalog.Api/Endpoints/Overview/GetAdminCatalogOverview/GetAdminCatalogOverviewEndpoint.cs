using Catalog.Application.Features.Overview.Queries.GetAdminCatalogOverview;
using Catalog.Contracts.Overview;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Overview.GetAdminCatalogOverview;

internal static class GetAdminCatalogOverviewEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminCatalogOverview")
            .WithSummary("Products without a photo and categories without site text")
            .WithDescription("Requires `CanManageProducts` (Manager, Seo, or Admin). Deleted products and categories are not counted.")
            .Produces<AdminCatalogOverviewResponse>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminCatalogOverviewQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}
