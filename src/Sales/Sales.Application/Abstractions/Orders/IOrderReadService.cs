using Sales.Application.Features.Orders.Queries.GetOrderById;
using Sales.Application.Features.Orders.Queries.GetOrderList;

namespace Sales.Application.Abstractions.Orders;

public interface IOrderReadService
{
    Task<Result<PagedResult<List<OrderListItemDto>>>> GetListAsync(
        OrderListRequest request,
        CancellationToken cancellationToken);

    Task<Result<OrderDetailDto>> GetByIdAsync(
        long orderId,
        CancellationToken cancellationToken);
}
