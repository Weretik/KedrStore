using Sales.Application.Features.Orders.GetSyncStatus.DTOs;

namespace Sales.Application.Contracts.Orders;

public interface IOrderSyncStatusReader
{
    Task<GetOrderSyncStatusResult?> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken);
}
