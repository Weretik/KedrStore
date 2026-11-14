using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.Orders.Commands.RetryOrderSync;

namespace Sales.Api.Endpoints.Orders;

internal static class RetryOrderSyncEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("{orderId:long}/sync/retry", Handle)
            .WithName("retryAdminOrderSync")
            .Produces<RetryAdminOrderSyncResponse>(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(
        [FromRoute] long orderId, [FromBody] RetryAdminOrderSyncRequest request, ClaimsPrincipal user,
        ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var requestedBy = user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? user.Identity?.Name
                          ?? "anonymous-development";
        var result = await sender.Send(new RetryOrderSyncCommand(orderId, request.Reason, requestedBy), cancellationToken);

        return result.Status == ResultStatus.Ok
            ? TypedResults.Json(
                new RetryAdminOrderSyncResponse(result.Value.OrderId, result.Value.OrderNumber,
                    result.Value.SyncStatus.ToString()),
                statusCode: StatusCodes.Status202Accepted)
            : result.ToProblem(http);
    }
}
