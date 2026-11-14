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
                $"Повторна відправка недоступна для статусу {result.PreviousStatus}."),
            OrderSyncRetryStoreOutcome.ConcurrencyConflict => Result.Conflict(
                "Стан синхронізації змінився, оновіть сторінку й спробуйте ще раз."),
            _ => Result.Error("Не вдалося запланувати повторну відправку.")
        };
    }
}
