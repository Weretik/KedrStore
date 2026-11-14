using Catalog.Application.Features.Orders.Commands.CreateOrder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Orders;

internal static class CreateOrderEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .AllowAnonymous()
            .RequireRateLimiting("QuickOrder")
            .Produces<QuickOrderResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

    private static async Task<IResult> Handle(
        [FromBody] CreateOrderRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateOrderCommand(request), cancellationToken);
        return result.ToHttpResult(http);
    }
}
