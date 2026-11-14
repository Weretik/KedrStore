using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.Outbox;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sales.Infrastructure.Persistence;

namespace IntegrationTests.Platform.Outbox;

/// <summary>Platform spec 006 on the Sales outbox table: same transaction, once only, retries, give-up, retention.</summary>
[Collection(SalesPostgresCollection.Name)]
public sealed class OutboxPostgresTests(SalesPostgresFixture postgres)
{
    private static readonly DateTimeOffset Start = new(2026, 11, 14, 10, 0, 0, TimeSpan.Zero);

    [SkippableFact]
    public async Task Message_SavedWithTheData_IsSentOnceRightAfterTheCommit()
    {
        await using var outbox = await CreateAsync();

        var messageId = await outbox.StoreAsync(new PingMessage("first"));
        await outbox.Dispatcher.DispatchAsync([messageId], CancellationToken.None);
        await outbox.Dispatcher.DispatchDueAsync(CancellationToken.None);

        Assert.Equal(["first"], outbox.Handler.Received);
        var stored = await outbox.LoadAsync(messageId);
        Assert.Equal(Start, stored.ProcessedAtUtc);
        Assert.Equal(1, stored.Attempts);
    }

    [SkippableFact]
    public async Task Message_NotSaved_WhenTheTransactionRollsBack()
    {
        await using var outbox = await CreateAsync();

        await using (var scope = outbox.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            scope.ServiceProvider.GetRequiredService<OutboxWriter>().Add(dbContext, new PingMessage("rolled back"));
            await dbContext.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        await outbox.Dispatcher.DispatchDueAsync(CancellationToken.None);
        Assert.Empty(outbox.Handler.Received);
    }

    [SkippableFact]
    public async Task FailedSend_StaysDue_AndTheNextDispatchSendsIt()
    {
        await using var outbox = await CreateAsync();
        outbox.Handler.FailuresLeft = 1;

        var messageId = await outbox.StoreAsync(new PingMessage("retry"));
        await outbox.Dispatcher.DispatchAsync([messageId], CancellationToken.None);

        var failed = await outbox.LoadAsync(messageId);
        Assert.Null(failed.ProcessedAtUtc);
        Assert.Null(failed.FailedAtUtc);
        Assert.Equal(1, failed.Attempts);
        Assert.Equal(Start.AddMinutes(1), failed.NextAttemptAtUtc);
        Assert.Equal("Telegram is down", failed.LastError);

        await outbox.Dispatcher.DispatchDueAsync(CancellationToken.None);
        Assert.Empty(outbox.Handler.Received);

        outbox.Clock.Advance(TimeSpan.FromMinutes(1));
        var report = await outbox.Dispatcher.DispatchDueAsync(CancellationToken.None);

        Assert.Equal(new OutboxDispatchReport(1, 0, 0), report);
        Assert.Equal(["retry"], outbox.Handler.Received);
        Assert.NotNull((await outbox.LoadAsync(messageId)).ProcessedAtUtc);
    }

    [SkippableFact]
    public async Task Message_FailingEveryAttempt_IsMarkedFailed_AndNotTriedAgain()
    {
        await using var outbox = await CreateAsync();
        outbox.Handler.FailuresLeft = int.MaxValue;

        var messageId = await outbox.StoreAsync(new PingMessage("broken"));
        for (var attempt = 0; attempt < OutboxRetrySchedule.MaximumAttempts + 2; attempt++)
        {
            await outbox.Dispatcher.DispatchDueAsync(CancellationToken.None);
            outbox.Clock.Advance(TimeSpan.FromHours(1));
        }

        var stored = await outbox.LoadAsync(messageId);
        Assert.Equal(OutboxRetrySchedule.MaximumAttempts, stored.Attempts);
        Assert.NotNull(stored.FailedAtUtc);
        Assert.Null(stored.ProcessedAtUtc);
        Assert.Equal(OutboxRetrySchedule.MaximumAttempts, outbox.Handler.Calls);
    }

    [SkippableFact]
    public async Task TwoDispatches_AtOnce_SendEachMessageOnce()
    {
        await using var outbox = await CreateAsync();
        outbox.Handler.Delay = TimeSpan.FromMilliseconds(300);
        await outbox.StoreAsync(new PingMessage("a"));
        await outbox.StoreAsync(new PingMessage("b"));

        await Task.WhenAll(
            outbox.Dispatcher.DispatchDueAsync(CancellationToken.None),
            outbox.Dispatcher.DispatchDueAsync(CancellationToken.None));

        Assert.Equal(["a", "b"], outbox.Handler.Received.Order());
    }

    [SkippableFact]
    public async Task ProcessedMessages_PastRetention_AreRemoved_FailedOnesStay()
    {
        await using var outbox = await CreateAsync();
        var sentId = await outbox.StoreAsync(new PingMessage("old"));
        await outbox.Dispatcher.DispatchAsync([sentId], CancellationToken.None);

        outbox.Clock.Advance(OutboxRetrySchedule.Retention + TimeSpan.FromMinutes(1));
        await outbox.Dispatcher.DispatchDueAsync(CancellationToken.None);

        await using var dbContext = postgres.CreateContext();
        Assert.False(await dbContext.Set<OutboxMessage>().AnyAsync(message => message.Id == sentId));
    }

    private async Task<OutboxScenario> CreateAsync()
    {
        postgres.SkipIfUnavailable();
        await using (var cleanup = postgres.CreateContext())
            await cleanup.Set<OutboxMessage>().ExecuteDeleteAsync();

        var clock = new ManualClock(Start);
        var handler = new PingHandler();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TimeProvider>(clock)
            .AddSingleton(handler)
            .AddDbContext<SalesDbContext>(options => options.UseNpgsql(postgres.ConnectionString))
            .AddOutbox<SalesDbContext>(SalesDbContext.Schema)
            .AddOutboxHandler<PingMessage, PingHandlerAdapter>("test.ping")
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        return new OutboxScenario(services, clock, handler);
    }

    private sealed record PingMessage(string Text) : IOutboxMessage;

    private sealed class PingHandler
    {
        private readonly List<string> _received = [];

        public int FailuresLeft { get; set; }

        public TimeSpan Delay { get; set; }

        public int Calls { get; private set; }

        public IReadOnlyList<string> Received
        {
            get { lock (_received) return _received.ToArray(); }
        }

        public async Task HandleAsync(PingMessage message)
        {
            Calls++;
            if (Delay > TimeSpan.Zero)
                await Task.Delay(Delay);
            if (FailuresLeft > 0)
            {
                FailuresLeft--;
                throw new HttpRequestException("Telegram is down");
            }

            lock (_received) _received.Add(message.Text);
        }
    }

    private sealed class PingHandlerAdapter(PingHandler handler) : IOutboxMessageHandler<PingMessage>
    {
        public Task HandleAsync(PingMessage message, Guid messageId, CancellationToken cancellationToken)
            => handler.HandleAsync(message);
    }

    private sealed class ManualClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    private sealed class OutboxScenario(ServiceProvider services, ManualClock clock, PingHandler handler) : IAsyncDisposable
    {
        public ServiceProvider Services { get; } = services;

        public ManualClock Clock { get; } = clock;

        public PingHandler Handler { get; } = handler;

        public IOutboxDispatcher Dispatcher => Services.GetRequiredService<IOutboxDispatcher>();

        public async Task<Guid> StoreAsync(PingMessage message)
        {
            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
            var id = scope.ServiceProvider.GetRequiredService<OutboxWriter>().Add(dbContext, message);
            await dbContext.SaveChangesAsync();
            return id;
        }

        public async Task<OutboxMessage> LoadAsync(Guid id)
        {
            await using var scope = Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<SalesDbContext>()
                .Set<OutboxMessage>().AsNoTracking().SingleAsync(message => message.Id == id);
        }

        public ValueTask DisposeAsync() => Services.DisposeAsync();
    }
}
