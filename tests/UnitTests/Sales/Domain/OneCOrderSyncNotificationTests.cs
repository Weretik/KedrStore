using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.Enums;
using Sales.Domain.Orders.ValueObjects;

namespace UnitTests.Sales.Domain;

public sealed class OneCOrderSyncNotificationTests
{
    [Fact]
    public void Single_notification_is_only_sent_once_per_dead_letter_transition()
    {
        var now = DateTimeOffset.UtcNow;
        var sync = OneCOrderSync.Create(OrderId.Create(1), now);
        sync.MoveToDeadLetter(now, "final failure");

        Assert.True(sync.TryStartDeadLetterNotification(now, maximumAttempts: 5));
        sync.MarkDeadLetterNotified(now.AddMinutes(1));

        Assert.False(sync.TryStartDeadLetterNotification(now.AddMinutes(2), maximumAttempts: 5));
        Assert.Equal(1, sync.DeadLetterNotificationAttemptCount);
        Assert.NotNull(sync.DeadLetterNotifiedAtUtc);
    }

    [Fact]
    public void Telegram_transport_failure_is_tracked_and_retried()
    {
        var now = DateTimeOffset.UtcNow;
        var sync = OneCOrderSync.Create(OrderId.Create(1), now);
        sync.MoveToDeadLetter(now, "final failure");

        Assert.True(sync.TryStartDeadLetterNotification(now, maximumAttempts: 5));
        sync.RegisterDeadLetterNotificationFailure(
            now,
            now.AddMinutes(5),
            maximumAttempts: 5,
            "Telegram transport failure: timeout");

        Assert.Equal(1, sync.DeadLetterNotificationAttemptCount);
        Assert.Equal(now.AddMinutes(5), sync.DeadLetterNotificationNextAttemptAtUtc);
        Assert.Equal("Telegram transport failure: timeout", sync.DeadLetterNotificationLastError);
        Assert.Null(sync.DeadLetterNotificationExhaustedAtUtc);
        Assert.True(sync.TryStartDeadLetterNotification(now.AddMinutes(5), maximumAttempts: 5));
    }

    [Fact]
    public void Notification_retries_are_bounded_and_do_not_change_dead_letter_state()
    {
        var now = DateTimeOffset.UtcNow;
        var sync = OneCOrderSync.Create(OrderId.Create(1), now);
        sync.MoveToDeadLetter(now, "final failure");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var attemptTime = now.AddMinutes(5 * attempt);
            Assert.True(sync.TryStartDeadLetterNotification(attemptTime, maximumAttempts: 5));
            sync.RegisterDeadLetterNotificationFailure(
                attemptTime,
                attemptTime.AddMinutes(5),
                maximumAttempts: 5,
                "Telegram unavailable");
        }

        Assert.Equal(OneCOrderSyncStatus.DeadLetter, sync.Status);
        Assert.Equal(5, sync.DeadLetterNotificationAttemptCount);
        Assert.NotNull(sync.DeadLetterNotificationExhaustedAtUtc);
        Assert.False(sync.TryStartDeadLetterNotification(now.AddDays(1), maximumAttempts: 5));
    }

    [Fact]
    public void Manual_retry_clears_previous_dead_letter_notification_cycle()
    {
        var now = DateTimeOffset.UtcNow;
        var sync = OneCOrderSync.Create(OrderId.Create(1), now);
        sync.MoveToDeadLetter(now, "final failure");
        Assert.True(sync.TryStartDeadLetterNotification(now, maximumAttempts: 5));
        sync.MarkDeadLetterNotified(now.AddMinutes(1));

        sync.ScheduleManualRetry(now.AddMinutes(2));

        Assert.Equal(OneCOrderSyncStatus.RetryScheduled, sync.Status);
        Assert.Equal(0, sync.DeadLetterNotificationAttemptCount);
        Assert.Null(sync.DeadLetterNotifiedAtUtc);
    }
}
