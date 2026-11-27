using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Orders.Sync;

namespace UnitTests.Sales.Application.Orders.OneCDelivery;

/// <summary>Platform spec 006, R-012: the 1C state the admin and the mobile app see.</summary>
public sealed class OrderOneCSyncTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void NewMessage_IsPending_WithItsNextAttempt()
    {
        var sync = OrderOneCSync.From(null, null, Message(attempts: 0, next: Now.AddHours(1)));

        Assert.Equal(new OrderOneCSync(OneCOrderSyncStatus.Pending, null, 0, null, Now.AddHours(1)), sync);
        Assert.False(sync.CanRetry);
    }

    [Fact]
    public void FailedAttempts_AreRetryScheduled_WithTheErrorAndTimes()
    {
        var sync = OrderOneCSync.From(null, null,
            Message(attempts: 2, next: Now.AddMinutes(5), last: Now, error: "1С недоступна"));

        Assert.Equal(new OrderOneCSync(OneCOrderSyncStatus.RetryScheduled, "1С недоступна", 2, Now, Now.AddMinutes(5)), sync);
        // Platform spec 006, R-015: «Надіслати зараз» instead of waiting for the next attempt.
        Assert.True(sync.CanRetry);
    }

    [Fact]
    public void GivenUpMessage_IsDeadLetter_AndCanBeRetried()
    {
        var sync = OrderOneCSync.From(null, null,
            Message(attempts: 10, next: Now, last: Now, failed: Now, error: "timeout"));

        Assert.Equal(OneCOrderSyncStatus.DeadLetter, sync.Status);
        Assert.Equal("timeout", sync.Error);
        Assert.Null(sync.NextAttemptAtUtc);
        Assert.True(sync.CanRetry);
    }

    [Fact]
    public void TheOneCAnswerOnTheOrder_Wins_EvenWhenRetentionRemovedTheMessage()
    {
        Assert.Equal(OneCOrderSyncStatus.Accepted, OrderOneCSync.From("DOC-1", null, null).Status);

        var rejected = OrderOneCSync.From(null, "Немає ціни", Message(attempts: 1, next: Now, processed: Now));
        Assert.Equal(OneCOrderSyncStatus.BusinessError, rejected.Status);
        Assert.Equal("Немає ціни", rejected.Error);
        Assert.True(rejected.CanRetry);
    }

    private static OneCMessageState Message(
        int attempts,
        DateTimeOffset next,
        DateTimeOffset? last = null,
        DateTimeOffset? processed = null,
        DateTimeOffset? failed = null,
        string? error = null)
        => new(attempts, next, last, processed, failed, error);
}
