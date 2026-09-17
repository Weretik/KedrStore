using Ardalis.Result;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Contracts.Catalog;
using Sales.Application.Contracts.Orders;
using Sales.Application.Contracts.Persistence;
using Sales.Application.Features.Orders.Create;
using Sales.Application.Features.Orders.Create.DTOs;
using Sales.Domain.Customers.Entities;
using Sales.Infrastructure.DataBase;
using Sales.Infrastructure.DependencyInjection;

namespace IntegrationTests.Sales.Persistence;

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
        var handler = new CreateOrderCommandHandler(
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
        services.AddSalesInfrastructureServices(configuration, includeCatalogReadServices: false);
        return services.BuildServiceProvider();
    }

    private async Task ResetDatabaseAsync()
    {
        await using var db = postgres.CreateContext();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"OrderSyncRetryAudits\"");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"OrderIdempotencyRecords\"");
        await db.OneCOrderSyncs.ExecuteDeleteAsync();
        await db.Orders.ExecuteDeleteAsync();
        await db.Counterparties.IgnoreQueryFilters().ExecuteDeleteAsync();
    }

    private async Task SeedCounterpartyAsync(string counterpartyId)
    {
        await using var db = postgres.CreateContext();
        db.Counterparties.Add(Counterparty.Create(
            counterpartyId,
            Guid.NewGuid(),
            "Create order customer",
            "create-order@example.com",
            "+380000000000",
            1,
            DateTimeOffset.UtcNow));
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
