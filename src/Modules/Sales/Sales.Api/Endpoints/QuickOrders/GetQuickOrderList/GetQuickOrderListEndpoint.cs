using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;
namespace Sales.Api.Endpoints.QuickOrders.GetQuickOrderList;

internal static class GetQuickOrderListEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminQuickOrders")
            .WithSummary("Browse orders sent from the site")
            .WithDescription("Newest first. Search by the words of a name or phone, or by an order number (the first 8 hex digits of the id, as on the site and in Telegram) or a whole id; delivery=problem keeps the orders Telegram has not got. Policy CanManageOrders.")
            .Produces<PagedResult<List<GetAdminQuickOrderListItemResponse>>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [AsParameters] GetAdminQuickOrdersRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetQuickOrderListQuery(new QuickOrderListRequest
            {
                Page = request.Page ?? 1,
                PageSize = request.PageSize ?? 20,
                Search = request.Search,
                Delivery = request.Delivery
            }),
            cancellationToken);

        if (result.Status != ResultStatus.Ok)
            return result.ToProblem(http);

        var rows = result.Value.Value
            .Select(order => new GetAdminQuickOrderListItemResponse(
                order.OrderId, order.PlacedAtUtc, order.FirstName, order.Phone, order.LineCount, order.Total,
                order.TelegramStatus.ToString().ToLowerInvariant(), order.TelegramLastError))
            .ToList();
        return TypedResults.Ok(new PagedResult<List<GetAdminQuickOrderListItemResponse>>(result.Value.PagedInfo, rows));
    }
}
