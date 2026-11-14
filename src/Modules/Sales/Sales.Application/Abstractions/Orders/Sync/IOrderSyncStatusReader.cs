using Sales.Application.Features.Orders.Queries.GetOrderSyncStatus;
namespace Sales.Application.Abstractions.Orders.Sync;

public interface IOrderSyncStatusReader
{
    Task<GetOrderSyncStatusResult?> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken);
}
