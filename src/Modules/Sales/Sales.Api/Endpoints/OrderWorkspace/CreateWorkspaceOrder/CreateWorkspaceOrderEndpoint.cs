using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceOrder;
using Sales.Api.Endpoints.OrderWorkspace.Shared;
namespace Sales.Api.Endpoints.OrderWorkspace.CreateWorkspaceOrder;

internal static class CreateWorkspaceOrderEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("orders", Handle)
            .WithName("createAdminWorkspaceOrder");

    // A missing key binds as empty, as it did in the controller; the validator answers 400.
    private static async Task<IResult> Handle(
        [FromBody] WorkspaceCreateOrderRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey = null)
    {
        var result = await sender.Send(
            new CreateWorkspaceOrderCommand(request, idempotencyKey ?? Guid.Empty), cancellationToken);
        if (!result.IsSuccess) return result.ToProblem(http);

        var created = result.Value;
        if (created.Conflict is not null) return created.Conflict.ToProblem(http);
        return created.Replay
            ? TypedResults.Ok(created.Order)
            : TypedResults.Json(created.Order, statusCode: StatusCodes.Status201Created);
    }
}
