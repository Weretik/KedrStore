using BuildingBlocks.Infrastructure.Outbox.Persistence;
using BuildingBlocks.Infrastructure.Persistence;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.OneCDelivery.Messages;
using Sales.Infrastructure.Persistence.Orders.SyncRetries;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Orders;

/// <summary>
/// «Повторити» (platform spec 006, R-013): clears the 1C refusal, puts the order's message back in the outbox and
/// writes the audit row in one transaction. The outbox requeue is a conditional update, so of two presses only one
/// wins; the other answers a conflict.
/// </summary>
internal sealed class OrderSyncRetryStore(
    SalesDbContext dbContext,
    OutboxWriter outbox,
    ILogger<OrderSyncRetryStore> logger) : IOrderSyncRetryStore
{
    public async Task<OrderSyncRetryStoreResult> ScheduleAsync(
        long orderId,
        string reason,
        string requestedBy,
        DateTimeOffset requestedAtUtc,
        CancellationToken cancellationToken)
    {
        var id = OrderId.FromStorage(orderId);
        var order = await dbContext.Orders.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
            return new OrderSyncRetryStoreResult(OrderSyncRetryStoreOutcome.NotFound, orderId, null, null);

        var message = await dbContext.Set<OutboxMessage>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == order.OneCMessageId, cancellationToken);
        var state = OrderOneCQueries.OneC(order.OneCDocumentNumber, order.OneCRejection, message);
        if (!state.CanRetry)
            return Result(OrderSyncRetryStoreOutcome.InvalidStatus, order, state.Status);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (!await outbox.RequeueAsync(dbContext, new OrderPlaced(orderId), order.OneCMessageId, cancellationToken)
            || order.ReopenOneCDelivery(requestedAtUtc).IsFailure)
            return Result(OrderSyncRetryStoreOutcome.ConcurrencyConflict, order, state.Status);

        dbContext.OrderSyncRetryAudits.Add(new OrderSyncRetryAudit
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            PreviousStatus = state.Status.ToString(),
            PreviousAttemptCount = state.Attempts,
            PreviousErrorMessage = state.Error,
            Reason = reason,
            RequestedBy = requestedBy,
            RequestedAtUtc = requestedAtUtc
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException error) when (UniqueViolation.Is(error))
        {
            // Retention had removed the message and another press stored it again first.
            dbContext.ChangeTracker.Clear();
            return Result(OrderSyncRetryStoreOutcome.ConcurrencyConflict, order, state.Status);
        }

        logger.LogInformation(
            "Manual 1C retry scheduled. OrderId={OrderId}, PreviousStatus={PreviousStatus}, RequestedBy={RequestedBy}.",
            orderId, state.Status, requestedBy);
        return Result(OrderSyncRetryStoreOutcome.Scheduled, order, state.Status);
    }

    private static OrderSyncRetryStoreResult Result(
        OrderSyncRetryStoreOutcome outcome,
        Order order,
        OneCOrderSyncStatus previousStatus)
        => new(outcome, order.Id.Value, order.OrderNumber, previousStatus);
}
