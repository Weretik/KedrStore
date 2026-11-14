using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.Queries.GetOrderSyncStatus;
using Sales.Application.Abstractions.Orders.Sync;

namespace Sales.Application.Features.Orders.Queries.GetOrderSyncStatus;

public sealed class GetOrderSyncStatusHandler(IOrderSyncStatusReader reader)
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
