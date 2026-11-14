using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.Orders.Commands.CreateOrder;

namespace Sales.Api.Endpoints.Orders;

internal static class CreateOrderEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("createAdminOrder")
            .Produces<CreateAdminOrderResponse>()
            .Produces<CreateAdminOrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    // A missing key binds as empty, as it did in the controller; the validator answers 400.
    private static async Task<IResult> Handle(
        [FromBody] CreateAdminOrderRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey = null)
    {
        var command = new CreateOrderCommand(
            new CreateOrderRequest(
                request.CounterpartyId,
                request.Comment,
                request.Lines.Select(line => new CreateOrderLineRequest(line.ProductId, line.Quantity, line.Amount)).ToArray()),
            idempotencyKey ?? Guid.Empty);
        var result = await sender.Send(command, cancellationToken);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(ToResponse(result.Value)),
            ResultStatus.Created => TypedResults.Json(ToResponse(result.Value), statusCode: StatusCodes.Status201Created),
            _ => result.ToProblem(http)
        };
    }

    private static CreateAdminOrderResponse ToResponse(CreateOrderResult result)
        => new(result.OrderId, result.OrderNumber, result.SyncStatus.ToString());
}
