using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.Orders.Queries.GetOrderSyncStatus;
namespace Sales.Api.Endpoints.Orders.GetOrderSyncStatus;

internal static class GetOrderSyncStatusEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{orderId:long}/sync-status", Handle)
            .WithName("getAdminOrderSyncStatus")
            .Produces<GetAdminOrderSyncStatusResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long orderId, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderSyncStatusQuery(orderId), cancellationToken);
        return result.Status == ResultStatus.Ok
            ? TypedResults.Ok(new GetAdminOrderSyncStatusResponse(
                result.Value.OrderId,
                result.Value.OrderNumber,
                result.Value.SyncStatus.ToString(),
                result.Value.OneCDocumentNumber,
                result.Value.AcceptedAtUtc))
            : result.ToProblem(http);
    }
}
