using BuildingBlocks.Domain.Identifiers;
using Ardalis.Result;
using Identity.Contracts.Users;
using IntegrationTests.TestSupport.Database;
using IntegrationTests.TestSupport.Sales;
using Microsoft.Extensions.DependencyInjection;
using Sales.Infrastructure.Customers;
using Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Sales.Application.Abstractions.Integrations.OneC;
using Sales.Domain.Customers;
using Sales.Domain.Orders;
using Sales.Infrastructure.Integrations.OneC;
using Sales.Infrastructure.Integrations.OneC.Counterparties;
namespace IntegrationTests.Sales.Persistence.Customers;

[Collection(SalesPostgresCollection.Name)]
public sealed class CounterpartyReconciliationPostgresTests(SalesPostgresFixture postgres)
{
    [SkippableFact]
    public async Task FullSnapshot_RemovesStaleCustomerWithOrderAndDraft_AndTheOutboxDeletesItsIdentityUser()
    {
        await using var sales = await SalesOutboxHarness.CreateAsync(postgres);
        var staleUserId = Guid.NewGuid();
        var retainedUserId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        await sales.InScopeAsync(async services =>
        {
            var db = services.GetRequiredService<SalesDbContext>();
            db.Counterparties.AddRange(
                Counterparty.Create("stale-cp", UserId.FromStorage(staleUserId), "Stale", "stale@example.com", null, PriceTypeId.FromStorage(1), now).Value,
                Counterparty.Create("retained-cp", UserId.FromStorage(retainedUserId), "Retained", "retained@example.com", null, PriceTypeId.FromStorage(1), now).Value);
            db.Orders.Add(Order.Create("P" + Guid.NewGuid().ToString("N")[..20], "stale-cp", null,
                [OrderLine.Create("101", "Door", 1, 25m).Value], now).Value);
            db.OrderDrafts.Add(OrderDraft.Create("Draft", null, "stale-cp",
                [OrderDraftLine.Create("101", 1)], now));
            return await db.SaveChangesAsync();
        });
        sales.Identity.FailuresLeft = 1;

        var first = await RunSyncAsync(sales);
        Assert.Equal(1, first.Deleted);
        await sales.InScopeAsync(async services =>
        {
            var db = services.GetRequiredService<SalesDbContext>();
            Assert.False(await db.Counterparties.IgnoreQueryFilters().AnyAsync(item => item.Id == CounterpartyId.FromStorage("stale-cp")));
            Assert.True(await db.Counterparties.AnyAsync(item => item.Id == CounterpartyId.FromStorage("retained-cp")));
            Assert.Empty(await db.Orders.ToArrayAsync());
            Assert.Empty(await db.OrderDrafts.ToArrayAsync());
            return true;
        });

        // Platform spec 006, R-014: the first deletion fails, the outbox retries it.
        await sales.Dispatcher.DispatchDueAsync(CancellationToken.None);
        Assert.Empty(sales.Identity.Deleted);
        Assert.Equal(1, Assert.Single(await sales.MessagesAsync(CounterpartyRemoved.TypeName)).Attempts);

        sales.Clock.Advance(TimeSpan.FromMinutes(1));
        await sales.Dispatcher.DispatchDueAsync(CancellationToken.None);
        Assert.Equal([staleUserId], sales.Identity.Deleted);

        var second = await RunSyncAsync(sales);
        Assert.Equal(0, second.Deleted);
        Assert.Equal(1, second.Unchanged);
        Assert.Single(await sales.MessagesAsync(CounterpartyRemoved.TypeName));
    }

    [SkippableFact]
    public async Task IdentityUser_LinkedToAnotherCustomerBeforeTheMessageRuns_IsKept()
    {
        await using var sales = await SalesOutboxHarness.CreateAsync(postgres);
        var sharedUserId = Guid.NewGuid();
        await sales.InScopeAsync(async services =>
        {
            var db = services.GetRequiredService<SalesDbContext>();
            db.Counterparties.Add(
                Counterparty.Create("stale-cp", UserId.FromStorage(sharedUserId), "Stale", "stale@example.com", null, PriceTypeId.FromStorage(1), DateTimeOffset.UtcNow).Value);
            return await db.SaveChangesAsync();
        });

        await RunSyncAsync(sales);
        await sales.InScopeAsync(async services =>
        {
            var db = services.GetRequiredService<SalesDbContext>();
            db.Counterparties.Add(
                Counterparty.Create("relinked-cp", UserId.FromStorage(sharedUserId), "Relinked", "relinked@example.com", null, PriceTypeId.FromStorage(1), DateTimeOffset.UtcNow).Value);
            return await db.SaveChangesAsync();
        });
        await sales.Dispatcher.DispatchDueAsync(CancellationToken.None);

        Assert.Empty(sales.Identity.Deleted);
        Assert.NotNull(Assert.Single(await sales.MessagesAsync(CounterpartyRemoved.TypeName)).ProcessedAtUtc);
    }

    private static Task<CounterpartiesSyncResult> RunSyncAsync(SalesOutboxHarness sales)
        => sales.InScopeAsync(services => new OneCCounterpartiesSyncService(new FakeOneC(),
                services.GetRequiredService<SalesDbContext>(),
                services.GetRequiredService<IImportedCounterpartyIdentityProvisioningService>(),
                services.GetRequiredService<StaleCounterpartyRemoval>(),
                new CounterpartyContactNormalizer(), NullLogger<OneCCounterpartiesSyncService>.Instance)
            .RunAsync(CancellationToken.None));

    private sealed class FakeOneC : ISalesOneCReadClient
    {
        public Task<IReadOnlyList<OneCCounterpartyDto>> GetCounterpartiesAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<OneCCounterpartyDto>>(
                [new("retained-cp", "Retained", "retained@example.com", null, 1)]);

        public Task<IReadOnlyList<OneCCounterpartyCategoryPriceTypeDto>> GetCounterpartyCategoryPriceTypesAsync(
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<OneCCounterpartyCategoryPriceTypeDto>>([]);
    }
}
