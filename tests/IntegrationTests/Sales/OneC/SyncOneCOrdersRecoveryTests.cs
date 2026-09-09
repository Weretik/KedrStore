using IntegrationTests.TestSupport.Sales;
using Sales.Application.Integrations.OneC.DTOs;
using Sales.Domain.Orders.Enums;
using Sales.Infrastructure.Integrations.OneC.Options;

namespace IntegrationTests.Sales.OneC;

public sealed class SyncOneCOrdersRecoveryTests
{
    [Fact]
    public async Task RunAsync_SecondPassDoesNotReclaimAlreadySentRecord()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now);
        var client = new RecordingWriteClient(OneCOrderDeliveryResult.Accepted("1C-123", null));
        var service = OrderSyncScenario.CreateService(db, client);

        await service.RunAsync(nowUtc: now);
        await service.RunAsync(nowUtc: now);

        Assert.Equal(1, client.CallCount);
        Assert.Equal(1, sync.AttemptCount);
    }

    [Fact]
    public async Task RunAsync_StaleSentOrder_IsSafelyRetriedWithTheSameOrderNumber()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now.AddHours(-1));
        sync.StartAttempt(now.AddMinutes(-16), "request-hash");
        await db.SaveChangesAsync();
        var client = new RecordingWriteClient(OneCOrderDeliveryResult.Accepted("1C-123", null));
        var service = OrderSyncScenario.CreateService(db, client);

        var result = await service.RunAsync(nowUtc: now);

        Assert.Equal(1, client.CallCount);
        Assert.Equal("SO-1", client.LastRequest!.OrderNumber);
        Assert.Equal(2, sync.AttemptCount);
        Assert.Equal(OneCOrderSyncStatus.Accepted, sync.Status);
        Assert.Equal(1, result.Accepted);
    }

    [Fact]
    public async Task RunAsync_RecentSentOrder_IsNotRetried()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now.AddHours(-1));
        sync.StartAttempt(now.AddMinutes(-5), "request-hash");
        await db.SaveChangesAsync();
        var client = new RecordingWriteClient(OneCOrderDeliveryResult.Accepted("1C-123", null));
        var service = OrderSyncScenario.CreateService(db, client);

        var result = await service.RunAsync(nowUtc: now);

        Assert.Equal(0, client.CallCount);
        Assert.Equal(1, sync.AttemptCount);
        Assert.Equal(OneCOrderSyncStatus.Sent, sync.Status);
        Assert.Equal(0, result.Processed);
    }

    [Fact]
    public async Task RunAsync_StaleSentOrderAtMaximumAttempts_MovesToDeadLetterWithoutSending()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now.AddHours(-1));
        for (var attempt = 1; attempt <= 10; attempt++)
            sync.StartAttempt(now.AddMinutes(-30 + attempt), "request-hash");
        await db.SaveChangesAsync();
        var client = new RecordingWriteClient(OneCOrderDeliveryResult.Accepted("unused", null));
        var service = OrderSyncScenario.CreateService(db, client);

        var result = await service.RunAsync(nowUtc: now);

        Assert.Equal(0, client.CallCount);
        Assert.Equal(10, sync.AttemptCount);
        Assert.Equal(OneCOrderSyncStatus.DeadLetter, sync.Status);
        Assert.Equal(1, result.DeadLetter);
    }

    [Fact]
    public async Task RunAsync_ConfiguredMaximumAttempts_MovesFailureToDeadLetter()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now.AddMinutes(-10));
        sync.StartAttempt(now.AddMinutes(-5), "request-hash");
        sync.RegisterTransportFailure(now.AddMinutes(-5), now, 10, "TransportError", "timeout", null);
        await db.SaveChangesAsync();
        var service = OrderSyncScenario.CreateService(
            db,
            OneCOrderDeliveryResult.TransportError("timeout"),
            new OneCOrderSyncOptions { MaximumAttempts = 2 });

        await service.RunAsync(nowUtc: now);

        Assert.Equal(2, sync.AttemptCount);
        Assert.Equal(OneCOrderSyncStatus.DeadLetter, sync.Status);
    }

    [Fact]
    public async Task RunAsync_TenthTransportFailure_MovesOrderToDeadLetter()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now.AddHours(-1));
        for (var attempt = 1; attempt <= 9; attempt++)
        {
            var attemptTime = now.AddMinutes(-20 + attempt);
            sync.StartAttempt(attemptTime, "request-hash");
            sync.RegisterTransportFailure(attemptTime, now, 10, "TransportError", "timeout", null);
        }

        await db.SaveChangesAsync();
        var client = new RecordingWriteClient(OneCOrderDeliveryResult.TransportError("timeout"));
        var service = OrderSyncScenario.CreateService(db, client);

        var result = await service.RunAsync(nowUtc: now);

        Assert.Equal(1, client.CallCount);
        Assert.Equal(10, sync.AttemptCount);
        Assert.Equal(OneCOrderSyncStatus.DeadLetter, sync.Status);
        Assert.Equal(1, result.DeadLetter);
    }
}
