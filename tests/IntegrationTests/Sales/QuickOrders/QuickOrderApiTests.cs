using System.Net;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox;
using IntegrationTests.TestSupport.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Infrastructure.Persistence;

namespace IntegrationTests.Sales.QuickOrders;

/// <summary>Platform spec 006, R-007: the site quick order is stored in the Sales outbox and sent from there.</summary>
[Collection(SalesPostgresCollection.Name)]
public sealed class QuickOrderApiTests(SalesPostgresFixture postgres)
{
    private static readonly DateTimeOffset Start = new(2026, 11, 14, 10, 0, 0, TimeSpan.Zero);

    [SkippableFact]
    public async Task QuickOrder_WhileTelegramWorks_IsSentAtOnce_AndTheRowIsProcessed()
    {
        await using var scenario = await CreateAsync();
        using var client = scenario.Factory.CreateClient();

        using var response = await client.PostAsync("/api/orders", Body(ValidOrder));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orderId = await OrderIdOf(response);
        var sent = Assert.Single(scenario.Telegram.Sent);
        Assert.Equal(orderId, sent.OrderId);
        Assert.Equal("Іван", sent.FirstName);
        Assert.Equal("+380670000000", sent.Phone);
        var line = Assert.Single(sent.Lines);
        Assert.Equal(new QuickOrderLine("101", "Ручка Кедр", 125.50m, 2), line);
        Assert.Equal(Start, sent.PlacedAtUtc);

        var row = Assert.Single(await scenario.RowsAsync());
        Assert.Equal(QuickOrderPlaced.TypeName, row.Type);
        Assert.Equal(Start, row.ProcessedAtUtc);
    }

    [SkippableFact]
    public async Task QuickOrder_WhileTelegramIsDown_Answers200_AndTheNextDispatchSendsIt()
    {
        await using var scenario = await CreateAsync();
        scenario.Telegram.Down = true;
        using var client = scenario.Factory.CreateClient();

        using var response = await client.PostAsync("/api/orders", Body(ValidOrder));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orderId = await OrderIdOf(response);
        Assert.Empty(scenario.Telegram.Sent);
        var waiting = Assert.Single(await scenario.RowsAsync());
        Assert.Null(waiting.ProcessedAtUtc);
        Assert.Null(waiting.FailedAtUtc);
        Assert.Equal(1, waiting.Attempts);
        Assert.Equal("Telegram is down", waiting.LastError);

        scenario.Telegram.Down = false;
        scenario.Clock.Advance(TimeSpan.FromMinutes(1));
        await scenario.Factory.Services.GetRequiredService<IOutboxDispatcher>().DispatchDueAsync(CancellationToken.None);

        Assert.Equal(orderId, Assert.Single(scenario.Telegram.Sent).OrderId);
        Assert.NotNull(Assert.Single(await scenario.RowsAsync()).ProcessedAtUtc);
    }

    [SkippableTheory]
    [InlineData("""{"firstName":"Іван","phone":"","lines":[{"productId":"101","title":"Ручка","unitPrice":1,"quantity":1}]}""", "Phone")]
    [InlineData("""{"firstName":"Іван","phone":"+380670000000","lines":[]}""", "Lines")]
    [InlineData("""{"firstName":"Іван","phone":"+380670000000"}""", "Lines")]
    public async Task InvalidQuickOrder_Answers400_AndStoresNothing(string body, string field)
    {
        await using var scenario = await CreateAsync();
        using var client = scenario.Factory.CreateClient();

        using var response = await client.PostAsync("/api/orders", Body(body));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty(field, out _));
        Assert.Empty(await scenario.RowsAsync());
        Assert.Empty(scenario.Telegram.Sent);
    }

    private const string ValidOrder =
        """{"firstName":"Іван","phone":"+380670000000","lines":[{"productId":"101","title":"Ручка Кедр","unitPrice":125.50,"quantity":2}]}""";

    private static StringContent Body(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task<Guid> OrderIdOf(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("orderId").GetGuid();
    }

    private async Task<Scenario> CreateAsync()
    {
        postgres.SkipIfUnavailable();
        await using (var cleanup = postgres.CreateContext())
            await cleanup.Set<OutboxMessage>().ExecuteDeleteAsync();

        var clock = new ManualClock(Start);
        var telegram = new FakeTelegram();
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:Default", postgres.ConnectionString);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(clock);
                services.RemoveAll<IOutboxMessageHandler<QuickOrderPlaced>>();
                services.AddSingleton(telegram);
                services.AddScoped<IOutboxMessageHandler<QuickOrderPlaced>, FakeTelegramHandler>();
            });
        });

        return new Scenario(factory, clock, telegram, postgres);
    }

    private sealed class FakeTelegram
    {
        private readonly List<QuickOrderPlaced> _sent = [];

        public bool Down { get; set; }

        public IReadOnlyList<QuickOrderPlaced> Sent
        {
            get { lock (_sent) return _sent.ToArray(); }
        }

        public void Send(QuickOrderPlaced order)
        {
            if (Down)
                throw new HttpRequestException("Telegram is down");
            lock (_sent) _sent.Add(order);
        }
    }

    private sealed class FakeTelegramHandler(FakeTelegram telegram) : IOutboxMessageHandler<QuickOrderPlaced>
    {
        public Task HandleAsync(QuickOrderPlaced message, Guid messageId, CancellationToken cancellationToken)
        {
            telegram.Send(message);
            return Task.CompletedTask;
        }
    }

    private sealed class ManualClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    private sealed class Scenario(
        WebApplicationFactory<Program> factory, ManualClock clock, FakeTelegram telegram, SalesPostgresFixture postgres)
        : IAsyncDisposable
    {
        public WebApplicationFactory<Program> Factory { get; } = factory;

        public ManualClock Clock { get; } = clock;

        public FakeTelegram Telegram { get; } = telegram;

        public async Task<IReadOnlyList<OutboxMessage>> RowsAsync()
        {
            await using SalesDbContext dbContext = postgres.CreateContext();
            return await dbContext.Set<OutboxMessage>().AsNoTracking().ToListAsync();
        }

        public ValueTask DisposeAsync() => Factory.DisposeAsync();
    }
}
