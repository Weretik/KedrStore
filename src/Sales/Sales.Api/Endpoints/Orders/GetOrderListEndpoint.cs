using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.Orders.Queries.GetOrderList;

namespace Sales.Api.Endpoints.Orders;

internal static class GetOrderListEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminOrders")
            .Produces<PagedResult<List<GetAdminOrderListItemResponse>>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [AsParameters] GetAdminOrdersRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetOrderListQuery(new OrderListRequest
            {
                CounterpartyId = request.CounterpartyId,
                Page = request.Page ?? 1,
                PageSize = request.PageSize ?? 20
            }),
            cancellationToken);

        if (result.Status != ResultStatus.Ok)
            return result.ToProblem(http);

        var rows = result.Value.Value.Select(ToResponse).ToList();
        return TypedResults.Ok(new PagedResult<List<GetAdminOrderListItemResponse>>(result.Value.PagedInfo, rows));
    }

    private static GetAdminOrderListItemResponse ToResponse(OrderListItemDto result)
        => new(
            result.OrderId,
            result.OrderNumber,
            result.CounterpartyId,
            result.CounterpartyName,
            result.CreatedAtUtc,
            result.LineCount,
            result.TotalAmount,
            result.SyncStatus.ToString(),
            result.OneCDocumentNumber);
}
