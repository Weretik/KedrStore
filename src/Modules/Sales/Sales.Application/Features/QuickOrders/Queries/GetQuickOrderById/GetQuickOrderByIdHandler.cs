using Sales.Domain.QuickOrders;
using Sales.Application.Abstractions.QuickOrders;

namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderById;

public sealed class GetQuickOrderByIdHandler(IQuickOrderReadService reader)
    : IQueryHandler<GetQuickOrderByIdQuery, Result<QuickOrderDetailDto>>
{
    public async ValueTask<Result<QuickOrderDetailDto>> Handle(
        GetQuickOrderByIdQuery query,
        CancellationToken cancellationToken) =>
        await reader.GetByIdAsync(query.OrderId, cancellationToken) is { } order
            ? Result.Success(order)
            : QuickOrderErrors.NotFound().ToResult<QuickOrderDetailDto>();
}
