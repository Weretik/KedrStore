using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.Enums;
using Sales.Domain.Orders.ValueObjects;

namespace UnitTests;

public sealed class OneCOrderSyncTests
{
    [Fact]
    public void Accept_MarksSyncAcceptedAndPreventsAnotherAttempt()
    {
        var now = DateTimeOffset.UtcNow;
        var sync = OneCOrderSync.Create(OrderId.Create(1), now);

        sync.StartAttempt(now, "payload-hash");
        sync.Accept(now.AddSeconds(1), "document-created");
        sync.StartAttempt(now.AddMinutes(1), "another-hash");

        Assert.Equal(OneCOrderSyncStatus.Accepted, sync.Status);
        Assert.Equal(1, sync.AttemptCount);
        Assert.NotNull(sync.AcceptedAtUtc);
    }

    [Fact]
    public void RegisterTransportFailure_SchedulesRetryBeforeMaximumAttempts()
    {
        var now = DateTimeOffset.UtcNow;
        var next = now.AddMinutes(5);
        var sync = OneCOrderSync.Create(OrderId.Create(1), now);

        sync.StartAttempt(now, "payload-hash");
        sync.RegisterTransportFailure(now, next, 10, "timeout", "1C did not respond");

        Assert.Equal(OneCOrderSyncStatus.RetryScheduled, sync.Status);
        Assert.Equal(next, sync.NextAttemptAtUtc);
        Assert.Equal("timeout", sync.LastErrorCode);
    }

    [Fact]
    public void RegisterTransportFailure_MovesToDeadLetterAtMaximumAttempts()
    {
        var now = DateTimeOffset.UtcNow;
        var sync = OneCOrderSync.Create(OrderId.Create(1), now);

        for (var attempt = 0; attempt < 10; attempt++)
        {
            sync.StartAttempt(now.AddMinutes(attempt), "payload-hash");
            sync.RegisterTransportFailure(now.AddMinutes(attempt), now.AddMinutes(attempt + 5), 10, "timeout", "1C did not respond");
        }

        Assert.Equal(OneCOrderSyncStatus.DeadLetter, sync.Status);
        Assert.Equal(10, sync.AttemptCount);
    }

    [Fact]
    public void MarkBusinessError_StopsAutomaticDelivery()
    {
        var now = DateTimeOffset.UtcNow;
        var sync = OneCOrderSync.Create(OrderId.Create(1), now);

        sync.StartAttempt(now, "payload-hash");
        sync.MarkBusinessError(now, "Counterparty not found");
        sync.StartAttempt(now.AddMinutes(5), "next-payload-hash");

        Assert.Equal(OneCOrderSyncStatus.BusinessError, sync.Status);
        Assert.Equal(1, sync.AttemptCount);
    }

    [Fact]
    public void Defer_SchedulesNextAllowedWindow()
    {
        var now = DateTimeOffset.UtcNow;
        var nextWindow = now.AddDays(1);
        var sync = OneCOrderSync.Create(OrderId.Create(1), now);

        sync.Defer(now, nextWindow);

        Assert.Equal(OneCOrderSyncStatus.RetryScheduled, sync.Status);
        Assert.Equal(nextWindow, sync.NextAttemptAtUtc);
    }
}
