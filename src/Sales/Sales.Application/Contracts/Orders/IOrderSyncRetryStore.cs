namespace Sales.Application.Contracts.Orders;

public interface IOrderSyncRetryStore
{
    Task<OrderSyncRetryStoreResult> ScheduleAsync(
        long orderId,
        string reason,
        string requestedBy,
        DateTimeOffset requestedAtUtc,
        CancellationToken cancellationToken);
}
