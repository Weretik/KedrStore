using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.GetById.DTOs;
using Sales.Application.Features.Orders.GetList.DTOs;
using Sales.Domain.Customers.Entities;
using Sales.Domain.Orders.Entities;
using Sales.Infrastructure.DependencyInjection;

namespace IntegrationTests.Sales.Persistence;

[Collection(SalesPostgresCollection.Name)]
public sealed class SalesOrderReadModelTests(SalesPostgresFixture postgres)
{
    [SkippableFact]
    public async Task GetListAsync_ProjectsPersistedOrdersNewestFirst()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var older = new DateTimeOffset(2026, 9, 9, 10, 0, 0, TimeSpan.Zero);
        var newer = older.AddHours(1);

        await SeedOrderAsync(
            "read-cp-1",
            "Older customer",
            older,
            "SO-OLDER",
            Enumerable.Repeat(1m, 150).ToArray());
        await SeedOrderAsync("read-cp-2", "Newer customer", newer, "SO-NEWER", 30m);

        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var reader = scope.ServiceProvider.GetRequiredService<IOrderReadService>();

        var result = await reader.GetListAsync(new OrderListRequest(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.PagedInfo.TotalRecords);
        Assert.Equal(["SO-NEWER", "SO-OLDER"], result.Value.Value.Select(row => row.OrderNumber));
        var olderRow = result.Value.Value[1];
        Assert.Equal(150, olderRow.LineCount);
        Assert.Equal(150m, olderRow.TotalAmount);
        Assert.Equal("Older customer", olderRow.CounterpartyName);
        Assert.Empty(scope.ServiceProvider.GetRequiredService<global::Sales.Infrastructure.DataBase.SalesDbContext>()
            .ChangeTracker.Entries());
    }

    [SkippableFact]
    public async Task GetListAsync_AppliesExactCounterpartyScopeAndPaging()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var createdAt = new DateTimeOffset(2026, 9, 9, 10, 0, 0, TimeSpan.Zero);
        await SeedOrderAsync("read-cp-1", "Customer one", createdAt, "SO-1", 10m);
        await SeedOrderAsync("read-cp-2", "Customer two", createdAt.AddMinutes(1), "SO-2", 20m);

        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var reader = scope.ServiceProvider.GetRequiredService<IOrderReadService>();

        var result = await reader.GetListAsync(
            new OrderListRequest { CounterpartyId = "read-cp-1", Page = 1, PageSize = 1 },
            CancellationToken.None);
        var unknown = await reader.GetListAsync(
            new OrderListRequest { CounterpartyId = "missing" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Value);
        Assert.Equal("read-cp-1", result.Value.Value[0].CounterpartyId);
        Assert.Equal(1, result.Value.PagedInfo.TotalRecords);
        Assert.True(unknown.IsSuccess);
        Assert.Empty(unknown.Value.Value);
        Assert.Equal(0, unknown.Value.PagedInfo.TotalPages);
    }

    [SkippableFact]
    public async Task GetByIdAsync_ReturnsSafeDetailForSoftDeletedCounterpartyWithoutChangingSync()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var createdAt = new DateTimeOffset(2026, 9, 9, 10, 0, 0, TimeSpan.Zero);
        var orderId = await SeedOrderAsync("read-cp-detail", "Historical customer", createdAt, "SO-DETAIL", 10m, 25m);
        await using (var db = postgres.CreateContext())
        {
            var counterparty = await db.Counterparties.SingleAsync();
            counterparty.MarkAsDeleted(createdAt.AddHours(1));
            var sync = await db.OneCOrderSyncs.SingleAsync();
            sync.StartAttempt(createdAt.AddMinutes(1), "private-payload-hash");
            sync.Accept(createdAt.AddMinutes(2), "1C-123", "private-response-body");
            await db.SaveChangesAsync();
        }

        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var reader = scope.ServiceProvider.GetRequiredService<IOrderReadService>();

        var result = await reader.GetByIdAsync(orderId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Historical customer", result.Value.Counterparty.Name);
        Assert.Equal([10m, 25m], result.Value.Lines.Select(line => line.Amount));
        Assert.Equal(35m, result.Value.TotalAmount);
        Assert.Equal("1C-123", result.Value.Sync.OneCDocumentNumber);
        Assert.DoesNotContain(typeof(OrderDetailDto).GetProperties(), property =>
            property.Name.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
            property.Name.Contains("Response", StringComparison.OrdinalIgnoreCase) ||
            property.Name.Contains("Attempt", StringComparison.OrdinalIgnoreCase));

        await using var verificationDb = postgres.CreateContext();
        var synchronization = await verificationDb.OneCOrderSyncs.SingleAsync();
        Assert.Equal(1, synchronization.AttemptCount);
        Assert.Equal("private-response-body", synchronization.OneCResponseBody);
    }

    [SkippableFact]
    public async Task GetByIdAsync_ReturnsNotFoundForUnknownOrder()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();

        var result = await scope.ServiceProvider.GetRequiredService<IOrderReadService>()
            .GetByIdAsync(404, CancellationToken.None);

        Assert.Equal(Ardalis.Result.ResultStatus.NotFound, result.Status);
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
        await db.OneCOrderSyncs.ExecuteDeleteAsync();
        await db.Orders.ExecuteDeleteAsync();
        await db.Counterparties.IgnoreQueryFilters().ExecuteDeleteAsync();
    }

    private async Task<long> SeedOrderAsync(
        string counterpartyId,
        string counterpartyName,
        DateTimeOffset createdAt,
        string orderNumber,
        params decimal[] amounts)
    {
        await using var db = postgres.CreateContext();
        var counterparty = Counterparty.Create(
            counterpartyId,
            Guid.NewGuid(),
            counterpartyName,
            $"{counterpartyId}@example.com",
            "+380000000000",
            1,
            createdAt);
        var lines = amounts.Select((amount, index) =>
            OrderLine.Create($"product-{index}", $"Product {index}", index + 1, amount));
        var order = Order.Create(orderNumber, counterpartyId, "Manager note", lines, createdAt);
        db.Counterparties.Add(counterparty);
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        db.OneCOrderSyncs.Add(OneCOrderSync.Create(order.Id, createdAt));
        await db.SaveChangesAsync();
        return order.Id.Value;
    }
}
