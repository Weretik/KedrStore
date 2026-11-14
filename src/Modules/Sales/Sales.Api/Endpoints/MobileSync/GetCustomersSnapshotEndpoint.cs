using BuildingBlocks.Api.Snapshots;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.MobileSync.Queries.GetCustomersSnapshot;

namespace Sales.Api.Endpoints.MobileSync;

internal static class GetCustomersSnapshotEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getMobileCustomersSnapshot")
            .Produces<CustomersSnapshotDto>()
            .Produces(StatusCodes.Status304NotModified)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetCustomersSnapshotQuery(), cancellationToken))
            .ToSnapshotHttpResult(http, snapshot => snapshot.Version);
}
