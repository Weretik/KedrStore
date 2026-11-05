using IntegrationTests.TestSupport.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sales.Application.Contracts.Orders;
using Sales.Domain.Orders.Entities;
using Sales.Infrastructure.DataBase;
using Sales.Infrastructure.DependencyInjection;

namespace IntegrationTests.Sales.Persistence;

/// <summary>Orders block of the admin «Огляд» on an in-memory sales store.</summary>
public sealed class WorkspaceOverviewReaderTests
{
    private static readonly DateTimeOffset TodayFrom = new(2026, 11, 4, 22, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Overview_CountsTodayAndDrafts_ListsRetryableSyncProblemsAndNewestOrders()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSalesInfrastructureServices(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = "Host=localhost;Database=unused" })
            .Build(), includeCatalogReadServices: false);
        services.RemoveAll<SalesDbContext>();
        services.RemoveAll<DbContextOptions<SalesDbContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<SalesDbContext>>();
        services.AddDbContext<SalesDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();

        var yesterday = await OrderSyncScenario.SeedOrderAsync(db, TodayFrom.AddHours(-3), "1");
        yesterday.StartAttempt(TodayFrom.AddHours(-2), "hash");
        yesterday.MarkBusinessError(TodayFrom.AddHours(-2), "Counterparty not found");
        var accepted = await OrderSyncScenario.SeedOrderAsync(db, TodayFrom.AddHours(1), "2");
        accepted.Accept(TodayFrom.AddHours(2), "DOC-2");
        await OrderSyncScenario.SeedOrderAsync(db, TodayFrom.AddHours(3), "3");
        db.OrderDrafts.Add(OrderDraft.Create("Чернетка", null, "cp-1", [OrderDraftLine.Create("p-1", 1)], TodayFrom));
        await db.SaveChangesAsync();

        var overview = await scope.ServiceProvider.GetRequiredService<IWorkspaceOverviewReader>()
            .GetAsync(TodayFrom, CancellationToken.None);

        Assert.Equal(2, overview.OrdersToday);
        Assert.Equal(1, overview.DraftCount);
        Assert.Equal(1, overview.SyncProblemCount);
        var problem = Assert.Single(overview.SyncProblems);
        Assert.Equal("SO-1", problem.OrderNumber);
        Assert.Equal("BusinessError", problem.SyncStatus);
        Assert.Equal("Counterparty not found", problem.ErrorMessage);
        Assert.Equal(["SO-3", "SO-2", "SO-1"], overview.LatestOrders.Select(order => order.OrderNumber));
        Assert.Equal(10m, overview.LatestOrders[0].TotalAmount);
    }
}
