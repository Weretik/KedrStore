using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.RetrySync;
using Sales.Domain.Orders.Enums;

namespace UnitTests.Sales.Application;

public sealed class RetryOrderSyncCommandHandlerTests
{
    [Fact]
    public async Task Handle_SchedulesTerminalFailureWithoutCallingOneC()
    {
        var store = new RecordingRetryStore(new OrderSyncRetryStoreResult(
            OrderSyncRetryStoreOutcome.Scheduled,
            42,
            "SO-42",
            OneCOrderSyncStatus.BusinessError));
        var handler = new RetryOrderSyncCommandHandler(store);

        var result = await handler.Handle(
            new RetryOrderSyncCommand(42, "  Reference data corrected  ", "manager-1"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OneCOrderSyncStatus.RetryScheduled, result.Value.SyncStatus);
        Assert.Equal("Reference data corrected", store.Reason);
        Assert.Equal("manager-1", store.RequestedBy);
    }

    [Theory]
    [InlineData(OneCOrderSyncStatus.Pending)]
    [InlineData(OneCOrderSyncStatus.Sent)]
    [InlineData(OneCOrderSyncStatus.Accepted)]
    [InlineData(OneCOrderSyncStatus.RetryScheduled)]
    public async Task Handle_ReturnsConflictWhenCurrentStatusCannotBeRetried(OneCOrderSyncStatus status)
    {
        var handler = new RetryOrderSyncCommandHandler(new RecordingRetryStore(
            new OrderSyncRetryStoreResult(OrderSyncRetryStoreOutcome.InvalidStatus, 42, "SO-42", status)));

        var result = await handler.Handle(
            new RetryOrderSyncCommand(42, "Retry requested", "manager-1"),
            CancellationToken.None);

        Assert.Equal(Ardalis.Result.ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task Handle_ReturnsNotFoundForUnknownOrder()
    {
        var handler = new RetryOrderSyncCommandHandler(new RecordingRetryStore(
            new OrderSyncRetryStoreResult(OrderSyncRetryStoreOutcome.NotFound, 404, null, null)));

        var result = await handler.Handle(
            new RetryOrderSyncCommand(404, "Retry requested", "manager-1"),
            CancellationToken.None);

        Assert.Equal(Ardalis.Result.ResultStatus.NotFound, result.Status);
    }

    private sealed class RecordingRetryStore(OrderSyncRetryStoreResult result) : IOrderSyncRetryStore
    {
        public string? Reason { get; private set; }
        public string? RequestedBy { get; private set; }

        public Task<OrderSyncRetryStoreResult> ScheduleAsync(
            long orderId,
            string reason,
            string requestedBy,
            DateTimeOffset requestedAtUtc,
            CancellationToken cancellationToken)
        {
            Reason = reason;
            RequestedBy = requestedBy;
            return Task.FromResult(result);
        }
    }
}
