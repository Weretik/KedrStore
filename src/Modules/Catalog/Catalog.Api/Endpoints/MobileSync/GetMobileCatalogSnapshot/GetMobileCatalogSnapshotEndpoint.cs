using Microsoft.AspNetCore.RateLimiting;
using BuildingBlocks.Api.Snapshots;
using Catalog.Application.Features.MobileSync.Queries.GetCatalogSnapshot;
using Catalog.Contracts.MobileSync;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.MobileSync.GetMobileCatalogSnapshot;

internal static class GetMobileCatalogSnapshotEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getMobileCatalogSnapshot").RequireRateLimiting("HeavyOperation")
            .WithSummary("Products, prices, categories and price types for offline work")
            .WithDescription("Requires `CanManageOrders` (Manager, Seo, or Admin); the app sends its bearer token. Deleted products and categories are left out. Responses carry a strong `ETag` (the same value as `version`) and are gzip or Brotli compressed when the client accepts it.")
            .Produces<CatalogSnapshotResponse>()
            .Produces(StatusCodes.Status304NotModified)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCatalogSnapshotQuery(), cancellationToken);
        return result.ToSnapshotHttpResult(http, snapshot => snapshot.Version);
    }
}
