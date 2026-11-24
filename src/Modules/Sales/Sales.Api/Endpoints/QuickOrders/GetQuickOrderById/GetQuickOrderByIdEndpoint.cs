using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.QuickOrders.Queries.GetQuickOrderById;
namespace Sales.Api.Endpoints.QuickOrders.GetQuickOrderById;

internal static class GetQuickOrderByIdEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{orderId:guid}", Handle)
            .WithName("getAdminQuickOrderById")
            .WithSummary("Get one order sent from the site")
            .WithDescription("The buyer, the cart lines and how the delivery to Telegram went. Policy CanManageOrders.")
            .Produces<GetAdminQuickOrderDetailResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetQuickOrderByIdQuery(orderId), cancellationToken);
        return result.Status == ResultStatus.Ok ? TypedResults.Ok(ToResponse(result.Value)) : result.ToProblem(http);
    }

    private static GetAdminQuickOrderDetailResponse ToResponse(QuickOrderDetailDto order) => new(
        order.OrderId,
        order.PlacedAtUtc,
        order.FirstName,
        order.Phone,
        order.Total,
        order.TelegramStatus.ToString().ToLowerInvariant(),
        order.TelegramSentAtUtc,
        order.TelegramLastAttemptAtUtc,
        order.TelegramLastError,
        order.Items
            .Select(item => new GetAdminQuickOrderItemResponse(item.ProductId, item.Title, item.UnitPrice, item.Quantity, item.Amount))
            .ToList());
}
