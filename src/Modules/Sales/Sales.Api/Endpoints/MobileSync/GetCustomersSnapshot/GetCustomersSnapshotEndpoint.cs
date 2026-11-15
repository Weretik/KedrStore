using BuildingBlocks.Api.Snapshots;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.MobileSync.Queries.GetCustomersSnapshot;
namespace Sales.Api.Endpoints.MobileSync.GetCustomersSnapshot;

internal static class GetCustomersSnapshotEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getMobileCustomersSnapshot")
            .WithSummary("Customers and their price rules for offline work")
            .WithDescription("Requires `CanManageOrders` (Manager, Seo, or Admin); the app sends its bearer token. Deleted customers are left out. Responses carry a strong `ETag` (the same value as `version`) and are gzip or Brotli compressed when the client accepts it.")
            .Produces<CustomersSnapshotDto>()
            .Produces(StatusCodes.Status304NotModified)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetCustomersSnapshotQuery(), cancellationToken))
            .ToSnapshotHttpResult(http, snapshot => snapshot.Version);
}
