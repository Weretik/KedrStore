using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.Queries.GetOrderList;

namespace Sales.Application.Features.Orders.Queries.GetOrderList;

public sealed class GetOrderListHandler(IOrderReadService reader)
    : IQueryHandler<GetOrderListQuery, Result<PagedResult<List<OrderListItemDto>>>>
{
    public async ValueTask<Result<PagedResult<List<OrderListItemDto>>>> Handle(
        GetOrderListQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await reader.GetListAsync(query.Request, cancellationToken);
    }
}
