using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.Outbox;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sales.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Outbox.Persistence;
using BuildingBlocks.Infrastructure.Outbox.Dispatch;

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
        Assert.Equal(Start, failed.LastAttemptAtUtc);

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
        Assert.Equal([messageId], outbox.GiveUps.Received.Select(x => x.MessageId));
        Assert.Equal(OutboxRetrySchedule.MaximumAttempts, outbox.GiveUps.Received.Single().Attempts);
    }

    [SkippableFact]
    public async Task Handler_AskingToRetryLater_DoesNotSpendAnAttempt()
    {
        await using var outbox = await CreateAsync();
        outbox.Handler.RetryAtUtc = Start.AddHours(9);

        var messageId = await outbox.StoreAsync(new PingMessage("night"));
        var report = await outbox.Dispatcher.DispatchDueAsync(CancellationToken.None);

        var stored = await outbox.LoadAsync(messageId);
        Assert.Equal(OutboxDispatchReport.Empty, report);
        Assert.Equal(0, stored.Attempts);
        Assert.Null(stored.LastAttemptAtUtc);
        Assert.Equal(Start.AddHours(9), stored.NextAttemptAtUtc);
        Assert.Null(stored.ProcessedAtUtc);
    }

    [SkippableFact]
    public async Task Requeue_PutsAFailedMessageBack_Once()
    {
        await using var outbox = await CreateAsync();
        outbox.Handler.FailuresLeft = int.MaxValue;
        var messageId = await outbox.StoreAsync(new PingMessage("again"));
        for (var attempt = 0; attempt < OutboxRetrySchedule.MaximumAttempts; attempt++)
        {
            await outbox.Dispatcher.DispatchDueAsync(CancellationToken.None);
            outbox.Clock.Advance(TimeSpan.FromHours(1));
        }

        Assert.True(await outbox.RequeueAsync(new PingMessage("again"), messageId));
        Assert.False(await outbox.RequeueAsync(new PingMessage("again"), messageId));

        var stored = await outbox.LoadAsync(messageId);
        Assert.Equal(0, stored.Attempts);
        Assert.Null(stored.FailedAtUtc);
        Assert.Null(stored.LastError);
        Assert.Equal(outbox.Clock.GetUtcNow(), stored.NextAttemptAtUtc);
    }

    [SkippableFact]
    public async Task Requeue_StoresTheMessageAgain_WhenRetentionRemovedIt()
    {
        await using var outbox = await CreateAsync();
        var messageId = Guid.CreateVersion7();

        Assert.True(await outbox.RequeueAsync(new PingMessage("gone"), messageId));
        await outbox.Dispatcher.DispatchDueAsync(CancellationToken.None);

        Assert.Equal(["gone"], outbox.Handler.Received);
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

    [SkippableFact]
    public async Task LaneMessage_WaitsForItsOwnDispatch_AndTheGeneralOnesNeverTouchIt()
    {
        await using var outbox = await CreateAsync();

        var laned = await outbox.StoreAsync(new LanedMessage("order"));
        var general = await outbox.StoreAsync(new PingMessage("telegram"));
        await outbox.Dispatcher.DispatchAsync([laned, general], CancellationToken.None);
        await outbox.Dispatcher.DispatchDueAsync(CancellationToken.None);

        Assert.Equal(["telegram"], outbox.Handler.Received);
        Assert.Null((await outbox.LoadAsync(laned)).ProcessedAtUtc);

        var report = await outbox.Dispatcher.DispatchLaneAsync(TestLane, CancellationToken.None);

        Assert.Equal(new OutboxDispatchReport(1, 0, 0), report);
        Assert.Equal(["telegram", "order"], outbox.Handler.Received);
        Assert.NotNull((await outbox.LoadAsync(laned)).ProcessedAtUtc);
    }

    private const string TestLane = "test-lane";

    private async Task<OutboxScenario> CreateAsync()
    {
        postgres.SkipIfUnavailable();
        await using (var cleanup = postgres.CreateContext())
            await cleanup.Set<OutboxMessage>().ExecuteDeleteAsync();

        var clock = new ManualClock(Start);
        var handler = new PingHandler();
        var giveUps = new PingGiveUps();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TimeProvider>(clock)
            .AddSingleton(handler)
            .AddSingleton(giveUps)
            .AddDbContext<SalesDbContext>(options => options.UseNpgsql(postgres.ConnectionString))
            .AddOutbox<SalesDbContext>(SalesDbContext.Schema)
            .AddOutboxHandler<PingMessage, PingHandlerAdapter>("test.ping")
            .AddOutboxHandler<LanedMessage, LanedHandlerAdapter>("test.laned", TestLane)
            .AddOutboxGiveUpHandler<PingMessage, PingGiveUpAdapter>()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        return new OutboxScenario(services, clock, handler, giveUps);
    }

    private sealed record PingMessage(string Text) : IOutboxMessage;

    private sealed record LanedMessage(string Text) : IOutboxMessage;

    private sealed class PingHandler
    {
        private readonly List<string> _received = [];

        public int FailuresLeft { get; set; }

        public TimeSpan Delay { get; set; }

        public DateTimeOffset? RetryAtUtc { get; set; }

        public int Calls { get; private set; }

        public IReadOnlyList<string> Received
        {
            get { lock (_received) return _received.ToArray(); }
        }

        public async Task<OutboxHandlingResult> HandleAsync(PingMessage message)
        {
            if (RetryAtUtc is { } retryAtUtc)
                return OutboxHandlingResult.RetryAt(retryAtUtc);
            Calls++;
            if (Delay > TimeSpan.Zero)
                await Task.Delay(Delay);
            if (FailuresLeft > 0)
            {
                FailuresLeft--;
                throw new HttpRequestException("Telegram is down");
            }

            lock (_received) _received.Add(message.Text);
            return OutboxHandlingResult.Delivered;
        }
    }

    private sealed class PingHandlerAdapter(PingHandler handler) : IOutboxMessageHandler<PingMessage>
    {
        public Task<OutboxHandlingResult> HandleAsync(PingMessage message, Guid messageId, CancellationToken cancellationToken)
            => handler.HandleAsync(message);
    }

    private sealed class LanedHandlerAdapter(PingHandler handler) : IOutboxMessageHandler<LanedMessage>
    {
        public Task<OutboxHandlingResult> HandleAsync(LanedMessage message, Guid messageId, CancellationToken cancellationToken)
            => handler.HandleAsync(new PingMessage(message.Text));
    }

    private sealed class PingGiveUps
    {
        public List<OutboxGiveUp> Received { get; } = [];
    }

    private sealed class PingGiveUpAdapter(PingGiveUps giveUps) : IOutboxGiveUpHandler<PingMessage>
    {
        public Task HandleGiveUpAsync(PingMessage message, OutboxGiveUp giveUp, CancellationToken cancellationToken)
        {
            giveUps.Received.Add(giveUp);
            return Task.CompletedTask;
        }
    }

    private sealed class ManualClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    private sealed class OutboxScenario(ServiceProvider services, ManualClock clock, PingHandler handler, PingGiveUps giveUps)
        : IAsyncDisposable
    {
        public PingGiveUps GiveUps { get; } = giveUps;

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

        public async Task<bool> RequeueAsync(PingMessage message, Guid id)
        {
            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
            var requeued = await scope.ServiceProvider.GetRequiredService<OutboxWriter>()
                .RequeueAsync(dbContext, message, id, CancellationToken.None);
            await dbContext.SaveChangesAsync();
            return requeued;
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
