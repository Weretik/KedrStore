using IntegrationTests.TestSupport.Database;
using IntegrationTests.TestSupport.Sales;
using Microsoft.EntityFrameworkCore;
using Sales.Application.Integrations.OneC.DTOs;
using Sales.Domain.Orders.Enums;

namespace IntegrationTests.Sales.OneC;

[Collection(SalesPostgresCollection.Name)]
public sealed class SyncOneCOrdersPostgresConcurrencyTests(SalesPostgresFixture postgres)
{
    [SkippableFact]
    public async Task TwoWorkers_SendOrderOnlyOnce()
    {
        postgres.SkipIfUnavailable();
        var now = new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero);

        await using (var cleanupDb = postgres.CreateContext())
        {
            await cleanupDb.OneCOrderSyncs.ExecuteDeleteAsync();
            await cleanupDb.Orders.ExecuteDeleteAsync();
            await cleanupDb.Counterparties.ExecuteDeleteAsync();
        }

        await using (var seedDb = postgres.CreateContext())
            await OrderSyncScenario.SeedOrderAsync(seedDb, now);

        var claimBarrier = new AsyncBarrier(2);
        await using var firstDb = postgres.CreateContext(new ConcurrentClaimBarrierInterceptor(claimBarrier));
        await using var secondDb = postgres.CreateContext(new ConcurrentClaimBarrierInterceptor(claimBarrier));
        var client = new RecordingWriteClient(OneCOrderDeliveryResult.Accepted("1C-123", null));
        var firstWorker = OrderSyncScenario.CreateService(firstDb, client);
        var secondWorker = OrderSyncScenario.CreateService(secondDb, client);

        await Task.WhenAll(
            firstWorker.RunAsync(nowUtc: now),
            secondWorker.RunAsync(nowUtc: now));

        await using var verificationDb = postgres.CreateContext();
        var sync = await verificationDb.OneCOrderSyncs.SingleAsync();
        Assert.Equal(1, client.CallCount);
        Assert.Equal(1, sync.AttemptCount);
        Assert.Equal(OneCOrderSyncStatus.Accepted, sync.Status);
    }
}
