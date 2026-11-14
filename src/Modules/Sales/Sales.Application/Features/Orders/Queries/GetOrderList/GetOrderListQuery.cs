using Sales.Application.Features.Orders.Queries.GetOrderList;

namespace Sales.Application.Features.Orders.Queries.GetOrderList;

public sealed record GetOrderListQuery(OrderListRequest Request)
    : IQuery<Result<PagedResult<List<OrderListItemDto>>>>;
