using IntegrationTests.TestSupport.Sales;
using Microsoft.EntityFrameworkCore;
using Sales.Application.Integrations.OneC.DTOs;
using Sales.Domain.Orders.Enums;
using Sales.Infrastructure.Integrations.OneC.Options;

namespace IntegrationTests.Sales.OneC;

public sealed class SyncOneCOrdersSchedulingTests
{
    [Fact]
    public async Task RunAsync_OutsideKyivWindow_DefersWithoutCallingOneC()
    {
        var now = new DateTimeOffset(2026, 9, 4, 3, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now);
        var client = new RecordingWriteClient(OneCOrderDeliveryResult.Accepted("1C-123", null));
        var service = OrderSyncScenario.CreateService(db, client);

        await service.RunAsync(nowUtc: now);

        Assert.Equal(0, client.CallCount);
        Assert.Equal(OneCOrderSyncStatus.RetryScheduled, sync.Status);
        Assert.True(sync.NextAttemptAtUtc > now);
    }

    [Fact]
    public async Task RunAsync_ConfiguredBatchSize_LimitsProcessedOrders()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        await OrderSyncScenario.SeedOrderAsync(db, now.AddSeconds(-1), "1");
        await OrderSyncScenario.SeedOrderAsync(db, now, "2");
        var client = new RecordingWriteClient(OneCOrderDeliveryResult.Accepted("1C-123", null));
        var service = OrderSyncScenario.CreateService(db, client, new OneCOrderSyncOptions { BatchSize = 1 });

        var result = await service.RunAsync(nowUtc: now);

        Assert.Equal(1, result.Processed);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task RunAsync_DefaultBatchSize_ProcessesOnlyTwentyOrders()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        for (var index = 1; index <= 21; index++)
            await OrderSyncScenario.SeedOrderAsync(db, now.AddSeconds(-index), index.ToString());

        var client = new RecordingWriteClient(OneCOrderDeliveryResult.Accepted("1C-123", null));
        var service = OrderSyncScenario.CreateService(
            db,
            client,
            new OneCOrderSyncOptions { InterCallDelay = TimeSpan.Zero });

        var result = await service.RunAsync(nowUtc: now);

        Assert.Equal(20, result.Processed);
        Assert.Equal(20, client.CallCount);
        Assert.Equal(1, await db.OneCOrderSyncs.CountAsync(sync => sync.Status == OneCOrderSyncStatus.Pending));
    }

    [Fact]
    public async Task RunAsync_ConfiguredZeroDelay_DoesNotWaitBetweenOrders()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        await OrderSyncScenario.SeedOrderAsync(db, now.AddSeconds(-1), "1");
        await OrderSyncScenario.SeedOrderAsync(db, now, "2");
        var client = new RecordingWriteClient(OneCOrderDeliveryResult.Accepted("1C-123", null));
        var service = OrderSyncScenario.CreateService(
            db,
            client,
            new OneCOrderSyncOptions { BatchSize = 2, InterCallDelay = TimeSpan.Zero });

        var started = DateTimeOffset.UtcNow;
        var result = await service.RunAsync(nowUtc: now);

        Assert.True(DateTimeOffset.UtcNow - started < TimeSpan.FromSeconds(2));
        Assert.Equal(2, result.Processed);
        Assert.Equal(2, client.CallCount);
    }

    [Fact]
    public async Task RunAsync_ConfiguredSendingWindow_DefersOutsideWindow()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now);
        var client = new RecordingWriteClient(OneCOrderDeliveryResult.Accepted("unused", null));
        var service = OrderSyncScenario.CreateService(
            db,
            client,
            new OneCOrderSyncOptions
            {
                SendingWindowStart = TimeSpan.FromHours(14),
                SendingWindowEnd = TimeSpan.FromHours(15)
            });

        await service.RunAsync(nowUtc: now);

        Assert.Equal(0, client.CallCount);
        Assert.Equal(OneCOrderSyncStatus.RetryScheduled, sync.Status);
    }
}
