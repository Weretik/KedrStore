using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.GetById.DTOs;

namespace Sales.Application.Features.Orders.GetById;

public sealed class GetOrderByIdQueryHandler(IOrderReadService reader)
    : IQueryHandler<GetOrderByIdQuery, Result<OrderDetailDto>>
{
    public async ValueTask<Result<OrderDetailDto>> Handle(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await reader.GetByIdAsync(query.OrderId, cancellationToken);
    }
}
