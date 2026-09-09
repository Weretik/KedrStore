using IntegrationTests.TestSupport.Database;
using IntegrationTests.TestSupport.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Contracts.Orders;
using Sales.Domain.Orders.Enums;
using Sales.Infrastructure.DataBase;
using Sales.Infrastructure.DependencyInjection;

namespace IntegrationTests.Sales.Persistence;

[Collection(SalesPostgresCollection.Name)]
public sealed class OrderSyncManualRetryPostgresTests(SalesPostgresFixture postgres)
{
    [SkippableFact]
    public async Task ScheduleAsync_PersistsRetryAndAuditAtomically()
    {
        postgres.SkipIfUnavailable();
        var now = new DateTimeOffset(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);

        await using (var cleanupDb = postgres.CreateContext())
        {
            await cleanupDb.Database.ExecuteSqlRawAsync("DELETE FROM \"OrderSyncRetryAudits\"");
            await cleanupDb.OneCOrderSyncs.ExecuteDeleteAsync();
            await cleanupDb.Orders.ExecuteDeleteAsync();
            await cleanupDb.Counterparties.ExecuteDeleteAsync();
        }

        long orderId;
        await using (var seedDb = postgres.CreateContext())
        {
            var sync = await OrderSyncScenario.SeedOrderAsync(seedDb, now.AddHours(-1));
            sync.StartAttempt(now.AddMinutes(-30), "payload-hash");
            sync.MarkBusinessError(now.AddMinutes(-29), "Counterparty not found");
            await seedDb.SaveChangesAsync();
            orderId = sync.OrderId.Value;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = postgres.ConnectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSalesInfrastructureServices(configuration);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IOrderSyncRetryStore>();

        var result = await store.ScheduleAsync(
            orderId,
            "Reference data corrected",
            "manager-1",
            now,
            CancellationToken.None);

        Assert.Equal(OrderSyncRetryStoreOutcome.Scheduled, result.Outcome);
        await using var verificationDb = postgres.CreateContext();
        var syncAfterRetry = await verificationDb.OneCOrderSyncs.SingleAsync();
        Assert.Equal(OneCOrderSyncStatus.RetryScheduled, syncAfterRetry.Status);
        Assert.Equal(0, syncAfterRetry.AttemptCount);
        Assert.Equal(now, syncAfterRetry.NextAttemptAtUtc);

        await verificationDb.Database.OpenConnectionAsync();
        await using var command = verificationDb.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT \"Reason\" FROM \"OrderSyncRetryAudits\" WHERE \"OrderId\" = @orderId";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "orderId";
        parameter.Value = orderId;
        command.Parameters.Add(parameter);
        Assert.Equal("Reference data corrected", await command.ExecuteScalarAsync());
    }
}
