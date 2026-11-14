using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.Orders.Queries.GetOrderById;

namespace Sales.Api.Endpoints.Orders;

internal static class GetOrderByIdEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{orderId:long}", Handle)
            .WithName("getAdminOrderById")
            .Produces<GetAdminOrderDetailResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long orderId, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        return result.Status == ResultStatus.Ok ? TypedResults.Ok(ToResponse(result.Value)) : result.ToProblem(http);
    }

    private static GetAdminOrderDetailResponse ToResponse(OrderDetailDto result)
        => new(
            result.OrderId,
            result.OrderNumber,
            result.CreatedAtUtc,
            new GetAdminOrderCounterpartyResponse(
                result.Counterparty.CounterpartyId,
                result.Counterparty.Name,
                result.Counterparty.Phone),
            result.Comment,
            result.Lines.Select(line => new GetAdminOrderLineResponse(
                line.ProductId,
                line.ProductName,
                line.Quantity,
                line.Amount)).ToList(),
            result.TotalAmount,
            new GetAdminOrderSyncResponse(
                result.Sync.Status.ToString(),
                result.Sync.OneCDocumentNumber,
                result.Sync.AcceptedAtUtc));
}
