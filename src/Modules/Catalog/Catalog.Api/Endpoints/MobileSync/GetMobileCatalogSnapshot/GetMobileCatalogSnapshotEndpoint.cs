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
            .WithName("getMobileCatalogSnapshot")
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
