using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;
using Sales.Application.Features.QuickOrders.Messages;
namespace Sales.Api.Endpoints.QuickOrders.PlaceQuickOrder;

internal static class PlaceQuickOrderEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .AllowAnonymous()
            .RequireRateLimiting("QuickOrder")
            .Produces<QuickOrderResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

    // Missing values bind as null; they become empty values here, and the validator answers 400.
    private static async Task<IResult> Handle(
        [FromBody] CreateOrderRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var lines = (request.Lines ?? [])
            .Select(line => line is null
                ? new QuickOrderLine(string.Empty, string.Empty, 0, 0)
                : new QuickOrderLine(line.ProductId, line.Title, line.UnitPrice, line.Quantity))
            .ToList();

        var result = await sender.Send(
            new PlaceQuickOrderCommand(request.FirstName, request.Phone, lines), cancellationToken);

        return result.IsSuccess ? TypedResults.Ok(new QuickOrderResponse(result.Value)) : result.ToProblem(http);
    }
}
