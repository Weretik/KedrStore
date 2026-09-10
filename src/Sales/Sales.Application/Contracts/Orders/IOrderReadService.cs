using Sales.Application.Features.Orders.GetById.DTOs;
using Sales.Application.Features.Orders.GetList.DTOs;

namespace Sales.Application.Contracts.Orders;

public interface IOrderReadService
{
    Task<Result<PagedResult<List<OrderListItemDto>>>> GetListAsync(
        OrderListRequest request,
        CancellationToken cancellationToken);

    Task<Result<OrderDetailDto>> GetByIdAsync(
        long orderId,
        CancellationToken cancellationToken);
}
