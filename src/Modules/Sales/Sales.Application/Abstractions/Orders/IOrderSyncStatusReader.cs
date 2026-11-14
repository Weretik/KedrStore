using Sales.Application.Features.Orders.Queries.GetOrderSyncStatus;

namespace Sales.Application.Abstractions.Orders;

public interface IOrderSyncStatusReader
{
    Task<GetOrderSyncStatusResult?> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken);
}
