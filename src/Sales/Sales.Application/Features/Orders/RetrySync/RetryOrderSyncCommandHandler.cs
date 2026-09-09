using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.RetrySync.DTOs;
using Sales.Domain.Orders.Enums;

namespace Sales.Application.Features.Orders.RetrySync;

public sealed class RetryOrderSyncCommandHandler(IOrderSyncRetryStore store)
    : ICommandHandler<RetryOrderSyncCommand, Result<RetryOrderSyncResult>>
{
    public async ValueTask<Result<RetryOrderSyncResult>> Handle(
        RetryOrderSyncCommand command,
        CancellationToken cancellationToken)
    {
        var result = await store.ScheduleAsync(
            command.OrderId,
            command.Reason.Trim(),
            command.RequestedBy,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return result.Outcome switch
        {
            OrderSyncRetryStoreOutcome.Scheduled => Result.Success(new RetryOrderSyncResult(
                result.OrderId,
                result.OrderNumber!,
                OneCOrderSyncStatus.RetryScheduled)),
            OrderSyncRetryStoreOutcome.NotFound => Result.NotFound(),
            OrderSyncRetryStoreOutcome.InvalidStatus => Result.Conflict(
                $"Manual retry is not allowed from status {result.PreviousStatus}."),
            OrderSyncRetryStoreOutcome.ConcurrencyConflict => Result.Conflict(
                "The synchronization state changed while the retry was being scheduled."),
            _ => Result.Error("Manual retry could not be scheduled.")
        };
    }
}
