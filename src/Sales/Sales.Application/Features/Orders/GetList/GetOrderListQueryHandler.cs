using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.GetList.DTOs;

namespace Sales.Application.Features.Orders.GetList;

public sealed class GetOrderListQueryHandler(IOrderReadService reader)
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
