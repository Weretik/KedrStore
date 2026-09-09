using IntegrationTests.TestSupport.Sales;
using Sales.Application.Integrations.OneC.DTOs;
using Sales.Domain.Orders.Enums;

namespace IntegrationTests.Sales.OneC;

public sealed class SyncOneCOrdersDeliveryTests
{
    [Fact]
    public async Task RunAsync_AcceptedResponse_PersistsAcceptedState()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now);
        var service = OrderSyncScenario.CreateService(db, OneCOrderDeliveryResult.Accepted("1C-123", null));

        var result = await service.RunAsync(nowUtc: now);

        Assert.Equal(1, result.Accepted);
        Assert.Equal(OneCOrderSyncStatus.Accepted, sync.Status);
        Assert.Equal("1C-123", sync.OneCDocumentNumber);
        Assert.Null(sync.OneCResponseBody);
    }

    [Fact]
    public async Task RunAsync_AcceptedResponseWithoutDocumentNumber_PersistsBusinessErrorWithoutNumber()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now);
        var service = OrderSyncScenario.CreateService(
            db,
            new OneCOrderDeliveryResult(OneCOrderDeliveryOutcome.Accepted, null, null));

        var result = await service.RunAsync(nowUtc: now);

        Assert.Equal(1, result.BusinessError);
        Assert.Equal(OneCOrderSyncStatus.BusinessError, sync.Status);
        Assert.Null(sync.OneCDocumentNumber);
    }

    [Fact]
    public async Task RunAsync_BusinessError_DoesNotPersistDocumentNumber()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now);
        var service = OrderSyncScenario.CreateService(db, OneCOrderDeliveryResult.BusinessError("OneCDocumentWasNotCreated"));

        await service.RunAsync(nowUtc: now);

        Assert.Null(sync.OneCDocumentNumber);
    }

    [Fact]
    public async Task RunAsync_BusinessError_PersistsBoundedDiagnostic()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now);
        var service = OrderSyncScenario.CreateService(db, OneCOrderDeliveryResult.BusinessError(new string('x', 2_000)));

        await service.RunAsync(nowUtc: now);

        Assert.Equal(OneCOrderSyncStatus.BusinessError, sync.Status);
        Assert.Equal(1_000, sync.LastErrorMessage!.Length);
    }

    [Fact]
    public async Task RunAsync_TransportFailure_SchedulesFiveMinuteRetryWithoutWaiting()
    {
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);
        await using var db = OrderSyncScenario.CreateInMemoryContext();
        var sync = await OrderSyncScenario.SeedOrderAsync(db, now);
        var service = OrderSyncScenario.CreateService(db, OneCOrderDeliveryResult.TransportError("timeout"));

        var started = DateTimeOffset.UtcNow;
        await service.RunAsync(nowUtc: now);

        Assert.True(DateTimeOffset.UtcNow - started < TimeSpan.FromSeconds(2));
        Assert.Equal(OneCOrderSyncStatus.RetryScheduled, sync.Status);
        Assert.Equal(now.AddMinutes(5), sync.NextAttemptAtUtc);
    }
}
