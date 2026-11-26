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
using Sales.Domain.QuickOrders;
using Sales.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Outbox.Persistence;

namespace IntegrationTests.Sales.QuickOrders;

/// <summary>
/// Platform spec 006, R-007 and spec sales/009: the site quick order is stored with its Telegram outbox message and sent
/// from there.
/// </summary>
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

        // Spec sales/009, R-001: the order itself is kept with its lines and total.
        var order = Assert.Single(await scenario.OrdersAsync());
        Assert.Equal(orderId, order.Id.Value);
        Assert.Equal(251.00m, order.Total);
        Assert.Equal(2, Assert.Single(order.Items).Quantity);
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

    [SkippableFact]
    public async Task RepeatedQuickOrder_WithTheSameIdempotencyKey_ReturnsTheFirstOrder_AndSendsOnce()
    {
        await using var scenario = await CreateAsync();
        using var client = scenario.Factory.CreateClient();
        var key = Guid.NewGuid();

        using var first = await client.SendAsync(Keyed(ValidOrder, key));
        using var repeat = await client.SendAsync(Keyed(ValidOrder, key));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeat.StatusCode);
        Assert.Equal(key, await OrderIdOf(first));
        Assert.Equal(key, await OrderIdOf(repeat));
        Assert.Equal(key, Assert.Single(await scenario.RowsAsync()).Id);
        Assert.Single(scenario.Telegram.Sent);
    }

    [SkippableFact]
    public async Task TwoCopiesAtOnce_WithTheSameIdempotencyKey_StoreOneOrder()
    {
        await using var scenario = await CreateAsync();
        using var client = scenario.Factory.CreateClient();
        var key = Guid.NewGuid();

        var answers = await Task.WhenAll(
            client.SendAsync(Keyed(ValidOrder, key)), client.SendAsync(Keyed(ValidOrder, key)));

        foreach (var answer in answers)
        {
            Assert.Equal(HttpStatusCode.OK, answer.StatusCode);
            Assert.Equal(key, await OrderIdOf(answer));
            answer.Dispose();
        }

        Assert.Single(await scenario.RowsAsync());
        Assert.Single(await scenario.OrdersAsync());
        Assert.Single(scenario.Telegram.Sent);
    }

    [SkippableFact]
    public async Task SameIdempotencyKey_WithOtherLines_Answers409_AndSendsNothingNew()
    {
        await using var scenario = await CreateAsync();
        using var client = scenario.Factory.CreateClient();
        var key = Guid.NewGuid();
        using var first = await client.SendAsync(Keyed(ValidOrder, key));

        using var other = await client.SendAsync(Keyed(ValidOrder.Replace("\"quantity\":2", "\"quantity\":3"), key));

        Assert.Equal(HttpStatusCode.Conflict, other.StatusCode);
        Assert.Single(await scenario.RowsAsync());
        Assert.Single(scenario.Telegram.Sent);
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
        Assert.Empty(await scenario.OrdersAsync());
        Assert.Empty(scenario.Telegram.Sent);
    }

    private const string ValidOrder =
        """{"firstName":"Іван","phone":"+380670000000","lines":[{"productId":"101","title":"Ручка Кедр","unitPrice":125.50,"quantity":2}]}""";

    private static StringContent Body(string json) => new(json, Encoding.UTF8, "application/json");

    private static HttpRequestMessage Keyed(string json, Guid key)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders") { Content = Body(json) };
        request.Headers.Add("Idempotency-Key", key.ToString());
        return request;
    }

    private static async Task<Guid> OrderIdOf(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("orderId").GetGuid();
    }

    private async Task<Scenario> CreateAsync()
    {
        postgres.SkipIfUnavailable();
        await using (var cleanup = postgres.CreateContext())
        {
            await cleanup.Set<OutboxMessage>().ExecuteDeleteAsync();
            await cleanup.Set<QuickOrder>().ExecuteDeleteAsync();
        }

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
        public Task<OutboxHandlingResult> HandleAsync(QuickOrderPlaced message, Guid messageId, CancellationToken cancellationToken)
        {
            telegram.Send(message);
            return Task.FromResult(OutboxHandlingResult.Delivered);
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
            // The Telegram messages; each order also stores its 1C message (R-010), tested in QuickOrderToOneCPostgresTests.
            return await dbContext.Set<OutboxMessage>().AsNoTracking()
                .Where(message => message.Type == QuickOrderPlaced.TypeName).ToListAsync();
        }

        public async Task<IReadOnlyList<QuickOrder>> OrdersAsync()
        {
            await using SalesDbContext dbContext = postgres.CreateContext();
            return await dbContext.Set<QuickOrder>().AsNoTracking().Include(order => order.Items).ToListAsync();
        }

        public ValueTask DisposeAsync() => Factory.DisposeAsync();
    }
}
