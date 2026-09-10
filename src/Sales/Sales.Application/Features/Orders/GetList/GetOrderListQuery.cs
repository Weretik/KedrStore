using Sales.Application.Features.Orders.GetList.DTOs;

namespace Sales.Application.Features.Orders.GetList;

public sealed record GetOrderListQuery(OrderListRequest Request)
    : IQuery<Result<PagedResult<List<OrderListItemDto>>>>;
