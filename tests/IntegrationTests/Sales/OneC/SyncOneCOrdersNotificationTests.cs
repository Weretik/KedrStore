using IntegrationTests.TestSupport.Sales;
using Sales.Application.Integrations.OneC.DTOs;

namespace IntegrationTests.Sales.OneC;

public sealed class SyncOneCOrdersNotificationTests
{
    [Fact]
    public async Task RunAsync_NoDueOrders_ProcessesPendingDeadLetterNotification()
    {
        var now = DateTimeOffset.UtcNow;
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now.AddMinutes(-10));
        sync.MoveToDeadLetter(now.AddMinutes(-9), "final transport failure");
        await db.SaveChangesAsync();

        var notifier = new RecordingNotifier();
        var service = OrderSyncScenario.CreateService(
            db,
            new RecordingWriteClient(OneCOrderDeliveryResult.Accepted("unused", null)),
            notifier);

        var result = await service.RunAsync(nowUtc: now);

        Assert.Equal(0, result.Processed);
        Assert.Equal(1, notifier.CallCount);
        Assert.NotNull(sync.DeadLetterNotifiedAtUtc);
    }
}
