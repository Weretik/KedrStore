using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.GetSyncStatus.DTOs;

namespace Sales.Application.Features.Orders.GetSyncStatus;

public sealed class GetOrderSyncStatusQueryHandler(IOrderSyncStatusReader reader)
    : IQueryHandler<GetOrderSyncStatusQuery, Result<GetOrderSyncStatusResult>>
{
    public async ValueTask<Result<GetOrderSyncStatusResult>> Handle(
        GetOrderSyncStatusQuery query,
        CancellationToken cancellationToken)
    {
        var result = await reader.GetByOrderIdAsync(query.OrderId, cancellationToken);
        return result is null ? Result.NotFound() : Result.Success(result);
    }
}
