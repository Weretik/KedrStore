using Sales.Application.Contracts.Orders;
using Sales.Domain.Orders.Enums;
using Sales.Domain.Orders.ValueObjects;
using Sales.Infrastructure.DataBase.Entities;

namespace Sales.Infrastructure.Orders;

internal sealed class OrderSyncRetryStore(
    SalesDbContext dbContext,
    ILogger<OrderSyncRetryStore> logger) : IOrderSyncRetryStore
{
    public async Task<OrderSyncRetryStoreResult> ScheduleAsync(
        long orderId,
        string reason,
        string requestedBy,
        DateTimeOffset requestedAtUtc,
        CancellationToken cancellationToken)
    {
        var candidate = await (
                from order in dbContext.Orders
                join sync in dbContext.OneCOrderSyncs on order.Id equals sync.OrderId
                where order.Id == OrderId.FromStorage(orderId)
                select new { Order = order, Sync = sync })
            .SingleOrDefaultAsync(cancellationToken);

        if (candidate is null)
            return new OrderSyncRetryStoreResult(OrderSyncRetryStoreOutcome.NotFound, orderId, null, null);

        var previousStatus = candidate.Sync.Status;
        if (previousStatus is not (OneCOrderSyncStatus.BusinessError or OneCOrderSyncStatus.DeadLetter))
        {
            return new OrderSyncRetryStoreResult(
                OrderSyncRetryStoreOutcome.InvalidStatus,
                orderId,
                candidate.Order.OrderNumber,
                previousStatus);
        }

        var audit = new OrderSyncRetryAudit
        {
            OneCOrderSyncId = candidate.Sync.Id,
            OrderId = candidate.Order.Id,
            OrderNumber = candidate.Order.OrderNumber,
            PreviousStatus = previousStatus.ToString(),
            PreviousAttemptCount = candidate.Sync.AttemptCount,
            PreviousErrorCode = candidate.Sync.LastErrorCode,
            PreviousErrorMessage = candidate.Sync.LastErrorMessage,
            Reason = reason,
            RequestedBy = requestedBy,
            RequestedAtUtc = requestedAtUtc
        };

        candidate.Sync.ScheduleManualRetry(requestedAtUtc);
        await dbContext.OrderSyncRetryAudits.AddAsync(audit, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogWarning(
                exception,
                "Manual 1C retry was not scheduled because the state changed concurrently. OrderId={OrderId}, PreviousStatus={PreviousStatus}.",
                orderId,
                previousStatus);
            return new OrderSyncRetryStoreResult(
                OrderSyncRetryStoreOutcome.ConcurrencyConflict,
                orderId,
                candidate.Order.OrderNumber,
                previousStatus);
        }

        logger.LogInformation(
            "Manual 1C retry scheduled. OrderId={OrderId}, PreviousStatus={PreviousStatus}, RequestedBy={RequestedBy}.",
            orderId,
            previousStatus,
            requestedBy);

        return new OrderSyncRetryStoreResult(
            OrderSyncRetryStoreOutcome.Scheduled,
            orderId,
            candidate.Order.OrderNumber,
            previousStatus);
    }
}
