using Ardalis.Result;
using Identity.Application.Services;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Sales.Application.Integrations.OneC.Contracts;
using Sales.Application.Integrations.OneC.DTOs;
using Sales.Domain.Customers.Entities;
using Sales.Domain.Orders.Entities;
using Sales.Infrastructure.Integrations.OneC.Services;

namespace IntegrationTests.Sales.Persistence;

[Collection(SalesPostgresCollection.Name)]
public sealed class CounterpartyReconciliationPostgresTests(SalesPostgresFixture postgres)
{
    [SkippableFact]
    public async Task FullSnapshot_RemovesStaleCustomerWithOrderAndDraft_AndRetriesIdentityDeletion()
    {
        postgres.SkipIfUnavailable();
        await using var db = postgres.CreateContext();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"PendingCounterpartyIdentityDeletions\"");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"OrderSyncRetryAudits\"");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"OrderIdempotencyRecords\"");
        await db.OneCOrderSyncs.ExecuteDeleteAsync();
        await db.Orders.ExecuteDeleteAsync();
        await db.OrderDrafts.ExecuteDeleteAsync();
        await db.CounterpartyCategoryPriceTypes.ExecuteDeleteAsync();
        await db.Counterparties.IgnoreQueryFilters().ExecuteDeleteAsync();

        var staleUserId = Guid.NewGuid();
        var retainedUserId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        db.Counterparties.AddRange(
            Counterparty.Create("stale-cp", staleUserId, "Stale", "stale@example.com", null, 1, now),
            Counterparty.Create("retained-cp", retainedUserId, "Retained", "retained@example.com", null, 1, now));
        db.Orders.Add(Order.Create("P" + Guid.NewGuid().ToString("N")[..20], "stale-cp", null,
            [OrderLine.Create("101", "Door", 1, 25m)], now));
        db.OrderDrafts.Add(OrderDraft.Create("Draft", null, "stale-cp",
            [OrderDraftLine.Create("101", 1)], now));
        await db.SaveChangesAsync();

        var identity = new FakeIdentity();
        var oneC = new FakeOneC();
        var remover = new StaleCounterpartyRemoval(db, identity,
            NullLogger<StaleCounterpartyRemoval>.Instance);
        var sync = new OneCCounterpartiesSyncService(oneC, db, identity, remover,
            new CounterpartyContactNormalizer(), NullLogger<OneCCounterpartiesSyncService>.Instance);

        var first = await sync.RunAsync(CancellationToken.None);
        Assert.Equal(1, first.Deleted);
        Assert.False(await db.Counterparties.IgnoreQueryFilters().AnyAsync(item => item.Id == "stale-cp"));
        Assert.True(await db.Counterparties.AnyAsync(item => item.Id == "retained-cp"));
        Assert.Empty(await db.Orders.ToArrayAsync());
        Assert.Empty(await db.OrderDrafts.ToArrayAsync());
        Assert.Equal(1, await PendingCountAsync(db));

        var second = await sync.RunAsync(CancellationToken.None);
        Assert.Equal(0, second.Imported);
        Assert.Equal(0, second.Deleted);
        Assert.Equal(1, second.Unchanged);
        Assert.Equal(0, await PendingCountAsync(db));
        Assert.Equal(2, identity.DeleteCalls);
    }

    private static Task<int> PendingCountAsync(global::Sales.Infrastructure.DataBase.SalesDbContext db)
        => db.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*)::integer AS \"Value\" FROM \"PendingCounterpartyIdentityDeletions\"")
            .SingleAsync();

    private sealed class FakeOneC : ISalesOneCReadClient
    {
        public Task<IReadOnlyList<OneCCounterpartyDto>> GetCounterpartiesAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<OneCCounterpartyDto>>(
                [new("retained-cp", "Retained", "retained@example.com", null, 1)]);

        public Task<IReadOnlyList<OneCCounterpartyCategoryPriceTypeDto>> GetCounterpartyCategoryPriceTypesAsync(
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<OneCCounterpartyCategoryPriceTypeDto>>([]);
    }

    private sealed class FakeIdentity : IImportedCounterpartyIdentityProvisioningService
    {
        public int DeleteCalls { get; private set; }

        public Task<Result<Guid>> EnsureUserAsync(ImportedCounterpartyIdentityUserData userData,
            Guid? existingIdentityUserId, CancellationToken cancellationToken)
            => Task.FromResult(Result.Success(existingIdentityUserId ?? Guid.NewGuid()));

        public Task<Result> DeleteUserAsync(Guid identityUserId, string counterpartyId,
            CancellationToken cancellationToken)
        {
            DeleteCalls++;
            return Task.FromResult(DeleteCalls == 1 ? Result.Error("temporary") : Result.Success());
        }
    }
}
