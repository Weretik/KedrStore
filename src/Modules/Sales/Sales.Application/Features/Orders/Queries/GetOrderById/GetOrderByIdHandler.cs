using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.Queries.GetOrderById;

namespace Sales.Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdHandler(IOrderReadService reader)
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
