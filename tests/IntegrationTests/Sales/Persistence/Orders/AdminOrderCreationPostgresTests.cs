using BuildingBlocks.Domain.Identifiers;
using Ardalis.Result;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Abstractions.Catalog;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Persistence;
using Sales.Application.Features.Orders.Commands.CreateOrder;
using Sales.Domain.Customers;
using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;
using Sales.Infrastructure.Persistence;
using Sales.Infrastructure.DependencyInjection;

namespace IntegrationTests.Sales.Persistence.Orders;

[Collection(SalesPostgresCollection.Name)]
public sealed class AdminOrderCreationPostgresTests(SalesPostgresFixture postgres)
{
    [SkippableFact]
    public async Task Create_ReturnsPersistedFinalOrderNumber_AndReplaysIt()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        const string counterpartyId = "create-order-cp";
        await SeedCounterpartyAsync(counterpartyId);

        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var handler = new CreateOrderHandler(
            scope.ServiceProvider.GetRequiredService<IOrderCreationStore>(),
            new ProductReader(),
            scope.ServiceProvider.GetRequiredService<IOrderNumberGenerator>());
        var command = new CreateOrderCommand(
            new CreateOrderRequest(
                counterpartyId,
                "Manager note",
                [new CreateOrderLineRequest("101", 2, 125m)]),
            Guid.NewGuid());

        var initial = await handler.Handle(command, CancellationToken.None);
        var replay = await handler.Handle(command, CancellationToken.None);

        await using var verificationDb = postgres.CreateContext();
        var persistedOrder = await verificationDb.Orders.AsNoTracking().SingleAsync();
        var expectedOrderNumber = $"SO-{persistedOrder.CreatedAt:yyyyMMdd}-{persistedOrder.Id.Value}";

        Assert.Equal(ResultStatus.Created, initial.Status);
        Assert.Equal(ResultStatus.Ok, replay.Status);
        Assert.Equal(expectedOrderNumber, persistedOrder.OrderNumber);
        Assert.Equal(persistedOrder.OrderNumber, initial.Value.OrderNumber);
        Assert.Equal(initial.Value.OrderNumber, replay.Value.OrderNumber);
        Assert.Matches("^SO-[0-9]{8}-[0-9]+$", initial.Value.OrderNumber);
        Assert.False(initial.Value.OrderNumber.StartsWith("P", StringComparison.Ordinal));
        Assert.False(replay.Value.OrderNumber.StartsWith("P", StringComparison.Ordinal));
    }

    /// <summary>
    /// Database rules 4 (audit 2026-11-15 finding G): a double submit that passes the replay check at the same time
    /// breaks the idempotency index; the second request answers with the first order instead of a 500.
    /// </summary>
    [SkippableFact]
    public async Task Create_WhenTheSameKeyWinsMeanwhile_ReplaysTheWinner()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        const string counterpartyId = "create-order-race";
        await SeedCounterpartyAsync(counterpartyId);
        var command = new CreateOrderCommand(
            new CreateOrderRequest(counterpartyId, null, [new CreateOrderLineRequest("101", 1, 50m)]), Guid.NewGuid());

        await using var provider = BuildProvider();
        await using var first = provider.CreateAsyncScope();
        var winner = await Handler(first.ServiceProvider, first.ServiceProvider.GetRequiredService<IOrderCreationStore>()).Handle(command,
            CancellationToken.None);
        await using var second = provider.CreateAsyncScope();
        var late = await Handler(second.ServiceProvider,
                new MissesTheFirstLookup(second.ServiceProvider.GetRequiredService<IOrderCreationStore>()))
            .Handle(command, CancellationToken.None);

        await using var verificationDb = postgres.CreateContext();
        Assert.Equal(ResultStatus.Created, winner.Status);
        Assert.Equal(ResultStatus.Ok, late.Status);
        Assert.Equal(winner.Value.OrderNumber, late.Value.OrderNumber);
        Assert.Equal(1, await verificationDb.Orders.CountAsync());
    }

    private static CreateOrderHandler Handler(IServiceProvider services, IOrderCreationStore store)
        => new(store, new ProductReader(), services.GetRequiredService<IOrderNumberGenerator>());

    /// <summary>The request that checked for a replay before the other one saved.</summary>
    private sealed class MissesTheFirstLookup(IOrderCreationStore inner) : IOrderCreationStore
    {
        private bool _missed;

        public Task<bool> CounterpartyExistsAsync(string counterpartyId, CancellationToken cancellationToken)
            => inner.CounterpartyExistsAsync(counterpartyId, cancellationToken);

        public Task<CreateOrderIdempotencyRecord?> FindIdempotencyRecordAsync(string operation, string idempotencyKey,
            CancellationToken cancellationToken)
        {
            if (_missed)
                return inner.FindIdempotencyRecordAsync(operation, idempotencyKey, cancellationToken);
            _missed = true;
            return Task.FromResult<CreateOrderIdempotencyRecord?>(null);
        }

        public Task<bool> PersistAsync(Order order,
            Func<OrderId, CreateOrderIdempotencyRecord> idempotencyRecordFactory,
            CancellationToken cancellationToken)
            => inner.PersistAsync(order, idempotencyRecordFactory, cancellationToken);
    }

    private ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = postgres.ConnectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSalesInfrastructure(configuration, includeCatalogReadServices: false);
        return services.BuildServiceProvider();
    }

    private async Task ResetDatabaseAsync()
    {
        await using var db = postgres.CreateContext();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM sales.\"OrderSyncRetryAudits\"");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM sales.\"OrderIdempotencyRecords\"");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM sales.\"OutboxMessages\"");
        await db.Orders.ExecuteDeleteAsync();
        await db.OrderDrafts.ExecuteDeleteAsync();
        await db.Counterparties.IgnoreQueryFilters().ExecuteDeleteAsync();
    }

    private async Task SeedCounterpartyAsync(string counterpartyId)
    {
        await using var db = postgres.CreateContext();
        db.Counterparties.Add(Counterparty.Create(
            counterpartyId,
            UserId.FromStorage(Guid.NewGuid()),
            "Create order customer",
            "create-order@example.com",
            "+380000000000",
            PriceTypeId.FromStorage(1),
            DateTimeOffset.UtcNow).Value);
        await db.SaveChangesAsync();
    }

    private sealed class ProductReader : IOrderProductReader
    {
        public Task<IReadOnlyDictionary<string, OrderProductSnapshot>> GetByIdsAsync(
            IReadOnlyCollection<string> productIds,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyDictionary<string, OrderProductSnapshot>>(
                productIds.ToDictionary(productId => productId, productId => new OrderProductSnapshot(productId, "Door")));
    }
}
