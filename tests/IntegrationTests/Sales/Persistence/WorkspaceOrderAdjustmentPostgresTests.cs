using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Contracts.Orders;
using Sales.Domain.Customers.Entities;
using Sales.Domain.Orders.Entities;
using Sales.Infrastructure.DependencyInjection;

namespace IntegrationTests.Sales.Persistence;

[Collection(SalesPostgresCollection.Name)]
public sealed class WorkspaceOrderAdjustmentPostgresTests(SalesPostgresFixture postgres)
{
    private const string CounterpartyId = "adjust-cp";

    [SkippableFact]
    public async Task Orders_ReportStoredAdjustmentsInListAndDetail()
    {
        postgres.SkipIfUnavailable();
        var orderId = await SeedOrderAsync();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = postgres.ConnectionString })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSalesInfrastructureServices(configuration, includeCatalogReadServices: false);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var reader = scope.ServiceProvider.GetRequiredService<IWorkspaceOrderReader>();

        var page = await reader.GetOrdersAsync(null, CounterpartyId, null, null, "-createdAt", 1, 20, CancellationToken.None);
        var detail = await reader.GetOrderAsync(orderId, CancellationToken.None);

        Assert.Equal(-70m, Assert.Single(page.Value).AdjustmentAmount);
        Assert.Equal([-50m, 10m], detail!.Lines.Select(x => x.PriceAdjustment));
        Assert.Equal(630m, detail.TotalAmount);
    }

    private async Task<long> SeedOrderAsync()
    {
        await using var db = postgres.CreateContext();
        await db.OrderDrafts.ExecuteDeleteAsync();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"PendingCounterpartyIdentityDeletions\"");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"OrderSyncRetryAudits\"");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"OrderIdempotencyRecords\"");
        await db.OneCOrderSyncs.ExecuteDeleteAsync();
        await db.Orders.ExecuteDeleteAsync();
        await db.CounterpartyCategoryPriceTypes.ExecuteDeleteAsync();
        await db.Counterparties.IgnoreQueryFilters().ExecuteDeleteAsync();
        var now = DateTimeOffset.UtcNow;
        db.Counterparties.Add(Counterparty.Create(CounterpartyId, Guid.NewGuid(), "Adjusted customer",
            "adjusted@example.com", null, 1, now));
        // 2 × (200 − 50) and 3 × (100 + 10): the stored amounts already include the adjustments.
        var order = Order.Create("SO-ADJ-1", CounterpartyId, null,
            [OrderLine.Create("101", "Товар 101", 2, 300m, -50m), OrderLine.Create("102", "Товар 102", 3, 330m, 10m)], now);
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        db.OneCOrderSyncs.Add(OneCOrderSync.Create(order.Id, now));
        await db.SaveChangesAsync();
        return order.Id.Value;
    }
}
