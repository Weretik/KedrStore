using Ardalis.Result;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox.Persistence;
using Identity.Contracts.Users;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sales.Application.Abstractions.DeliveryFailure;
using Sales.Application.Abstractions.Integrations.OneC;
using Sales.Infrastructure.DependencyInjection;
using Sales.Infrastructure.Persistence;

namespace IntegrationTests.TestSupport.Sales;

/// <summary>
/// The Sales module with its real outbox on the test PostgreSQL, and fakes at the edges: 1C, Telegram, Identity and the
/// clock (platform spec 006, stage 2).
/// </summary>
internal sealed class SalesOutboxHarness : IAsyncDisposable
{
    // 12:00 in Kyiv: inside the 1C sending window.
    public static readonly DateTimeOffset Noon = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);

    private SalesOutboxHarness(ServiceProvider services) => Services = services;

    public ServiceProvider Services { get; }

    public ManualClock Clock => Services.GetRequiredService<ManualClock>();

    public FakeOneCWriter OneC => Services.GetRequiredService<FakeOneCWriter>();

    public FakeDeliveryFailureNotifier Telegram => Services.GetRequiredService<FakeDeliveryFailureNotifier>();

    public FakeIdentityUsers Identity => Services.GetRequiredService<FakeIdentityUsers>();

    public IOutboxDispatcher Dispatcher => Services.GetRequiredService<IOutboxDispatcher>();

    public static async Task<SalesOutboxHarness> CreateAsync(SalesPostgresFixture postgres)
    {
        postgres.SkipIfUnavailable();
        await using (var db = postgres.CreateContext())
        {
            await db.Database.ExecuteSqlRawAsync("""
                DELETE FROM sales."OrderSyncRetryAudits";
                DELETE FROM sales."OrderIdempotencyRecords";
                DELETE FROM sales."OutboxMessages";
                """);
            await db.Orders.ExecuteDeleteAsync();
            await db.OrderDrafts.ExecuteDeleteAsync();
            await db.CounterpartyCategoryPriceTypes.ExecuteDeleteAsync();
            await db.Counterparties.IgnoreQueryFilters().ExecuteDeleteAsync();
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = postgres.ConnectionString })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSalesInfrastructure(configuration, includeCatalogReadServices: false);
        services.AddSingleton(new ManualClock(Noon));
        services.Replace(ServiceDescriptor.Singleton<TimeProvider>(sp => sp.GetRequiredService<ManualClock>()));
        services.AddSingleton<FakeOneCWriter>();
        services.Replace(ServiceDescriptor.Scoped<ISalesOneCWriteClient>(sp => sp.GetRequiredService<FakeOneCWriter>()));
        services.AddSingleton<FakeDeliveryFailureNotifier>();
        services.Replace(ServiceDescriptor.Scoped<IDeadLetterNotifier>(sp => sp.GetRequiredService<FakeDeliveryFailureNotifier>()));
        services.AddSingleton<FakeIdentityUsers>();
        services.Replace(ServiceDescriptor.Scoped<IImportedCounterpartyIdentityProvisioningService>(
            sp => sp.GetRequiredService<FakeIdentityUsers>()));
        return new SalesOutboxHarness(services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true }));
    }

    public async Task<T> InScopeAsync<T>(Func<IServiceProvider, Task<T>> work)
    {
        await using var scope = Services.CreateAsyncScope();
        return await work(scope.ServiceProvider);
    }

    public Task<OutboxMessage[]> MessagesAsync(string type) => InScopeAsync(services =>
        services.GetRequiredService<SalesDbContext>().Set<OutboxMessage>().AsNoTracking()
            .Where(message => message.Type == type).ToArrayAsync());

    public ValueTask DisposeAsync() => Services.DisposeAsync();
}

internal sealed class ManualClock(DateTimeOffset now) : TimeProvider
{
    private DateTimeOffset _now = now;

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now += by;
}

internal sealed class FakeOneCWriter : ISalesOneCWriteClient
{
    public Queue<OneCOrderDeliveryResult> Answers { get; } = new();

    public OneCOrderDeliveryResult Otherwise { get; set; } = OneCOrderDeliveryResult.TransportError("1C is down");

    public List<OneCOrderDeliveryRequest> Sent { get; } = [];

    public Task<OneCOrderDeliveryResult> SendOrderAsync(OneCOrderDeliveryRequest request, CancellationToken cancellationToken)
    {
        lock (Sent) Sent.Add(request);
        return Task.FromResult(Answers.TryDequeue(out var answer) ? answer : Otherwise);
    }
}

internal sealed class FakeDeliveryFailureNotifier : IDeadLetterNotifier
{
    public List<DeadLetterNotification> Sent { get; } = [];

    public Task SendAsync(DeadLetterNotification notification, DeadLetterExcelFile attachment, CancellationToken cancellationToken = default)
    {
        Sent.Add(notification);
        return Task.CompletedTask;
    }
}

internal sealed class FakeIdentityUsers : IImportedCounterpartyIdentityProvisioningService
{
    public int FailuresLeft { get; set; }

    public List<Guid> Deleted { get; } = [];

    public Task<Result<Guid>> EnsureUserAsync(ImportedCounterpartyIdentityUserData userData, Guid? existingIdentityUserId,
        CancellationToken cancellationToken)
        => Task.FromResult(Result.Success(existingIdentityUserId ?? Guid.NewGuid()));

    public Task<Result> DeleteUserAsync(Guid identityUserId, string counterpartyId, CancellationToken cancellationToken)
    {
        if (FailuresLeft > 0)
        {
            FailuresLeft--;
            return Task.FromResult(Result.Error("temporary"));
        }

        Deleted.Add(identityUserId);
        return Task.FromResult(Result.Success());
    }
}
