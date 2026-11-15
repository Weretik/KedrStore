using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;
using Sales.Application.Features.QuickOrders.Messages;
namespace Sales.Api.Endpoints.QuickOrders.PlaceQuickOrder;

internal static class PlaceQuickOrderEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("createQuickOrder")
            .WithSummary("Create a quick order")
            .WithDescription("Anonymous. Creates a quick order. Send an Idempotency-Key made once per order attempt and reuse it on a retry: a repeat returns the same order id, and the same key with other data answers 409.")
            .AllowAnonymous()
            .RequireRateLimiting("QuickOrder")
            .Produces<QuickOrderResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

    // Missing values bind as null; they become empty values here, and the validator answers 400. The optional
    // Idempotency-Key is made by the site once per order attempt: a repeat returns the same order id (409 when the
    // same key comes with other data).
    private static async Task<IResult> Handle(
        [FromBody] CreateOrderRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey = null)
    {
        var lines = (request.Lines ?? [])
            .Select(line => line is null
                ? new QuickOrderLine(string.Empty, string.Empty, 0, 0)
                : new QuickOrderLine(line.ProductId, line.Title, line.UnitPrice, line.Quantity))
            .ToList();

        var result = await sender.Send(
            new PlaceQuickOrderCommand(request.FirstName, request.Phone, lines, idempotencyKey), cancellationToken);

        return result.IsSuccess ? TypedResults.Ok(new QuickOrderResponse(result.Value)) : result.ToProblem(http);
    }
}
