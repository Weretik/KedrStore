using System.Net;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox.Persistence;
using IntegrationTests.TestSupport.Api;
using IntegrationTests.TestSupport.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sales.Domain.QuickOrders;
using Telegram.Bot;

namespace IntegrationTests.Sales.QuickOrders;

/// <summary>
/// Spec sales/009: the real Telegram sender over a fake Telegram HTTP API records the delivery state on the order; the
/// Admin lists, opens and resends site orders (SC-001, SC-002, SC-005…SC-008).
/// </summary>
[Collection(SalesPostgresCollection.Name)]
public sealed class QuickOrderAdminApiTests(SalesPostgresFixture postgres)
{
    private static readonly DateTimeOffset Start = new(2026, 11, 24, 7, 45, 0, TimeSpan.Zero);
    private const string Kicked = "Forbidden: bot was kicked from the group chat";

    [SkippableFact]
    public async Task DeliveredOrder_IsListedAsSent_WithItsLines()
    {
        await using var scenario = await CreateAsync();
        using var site = scenario.Factory.CreateClient();
        var orderId = await PlaceAsync(site, "Ірина", "+380954315238");
        using var admin = scenario.Factory.CreateClientWithRole("Manager");

        using var list = await JsonOf(await admin.GetAsync("/api/admin/quick-orders"));
        var row = Assert.Single(list.RootElement.GetProperty("value").EnumerateArray());
        Assert.Equal(orderId, row.GetProperty("orderId").GetGuid());
        Assert.Equal("sent", row.GetProperty("telegramStatus").GetString());
        Assert.Equal(JsonValueKind.Null, row.GetProperty("telegramLastError").ValueKind);
        Assert.Equal(1, row.GetProperty("lineCount").GetInt32());
        Assert.Equal(251.00m, row.GetProperty("total").GetDecimal());

        using var detail = await JsonOf(await admin.GetAsync($"/api/admin/quick-orders/{orderId}"));
        Assert.Equal("Ірина", detail.RootElement.GetProperty("firstName").GetString());
        var item = Assert.Single(detail.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal("Ручка Кедр", item.GetProperty("title").GetString());
        Assert.Equal(251.00m, item.GetProperty("amount").GetDecimal());
    }

    [SkippableFact]
    public async Task OrderTelegramRefuses_IsAProblem_UntilAPersonResendsIt()
    {
        await using var scenario = await CreateAsync();
        scenario.Telegram.Refuse = Kicked;
        using var site = scenario.Factory.CreateClient();
        var orderId = await PlaceAsync(site, "Ірина", "+380954315238");
        using var admin = scenario.Factory.CreateClientWithRole("Manager");

        // SC-002: the buyer got the id; the order waits with the reason.
        using (var pending = await JsonOf(await admin.GetAsync("/api/admin/quick-orders?delivery=problem")))
        {
            var row = Assert.Single(pending.RootElement.GetProperty("value").EnumerateArray());
            Assert.Equal("pending", row.GetProperty("telegramStatus").GetString());
            Assert.Equal(Kicked, row.GetProperty("telegramLastError").GetString());
        }

        using (var early = await admin.PostAsync($"/api/admin/quick-orders/{orderId}/telegram-resend", null))
            Assert.Equal(HttpStatusCode.Conflict, early.StatusCode);

        await scenario.ExhaustRetriesAsync();
        using (var failed = await JsonOf(await admin.GetAsync($"/api/admin/quick-orders/{orderId}")))
            Assert.Equal("failed", failed.RootElement.GetProperty("telegramStatus").GetString());

        // SC-007: a person resends after the chat is fixed; a second press sends nothing twice.
        scenario.Telegram.Refuse = null;
        using (var resend = await admin.PostAsync($"/api/admin/quick-orders/{orderId}/telegram-resend", null))
            Assert.Equal(HttpStatusCode.Accepted, resend.StatusCode);
        using (var repeat = await admin.PostAsync($"/api/admin/quick-orders/{orderId}/telegram-resend", null))
            Assert.Equal(HttpStatusCode.Conflict, repeat.StatusCode);

        await scenario.DispatchAsync();
        using var sent = await JsonOf(await admin.GetAsync($"/api/admin/quick-orders/{orderId}"));
        Assert.Equal("sent", sent.RootElement.GetProperty("telegramStatus").GetString());
        Assert.Equal(1, scenario.Telegram.Delivered);
        using var problems = await JsonOf(await admin.GetAsync("/api/admin/quick-orders?delivery=problem"));
        Assert.Empty(problems.RootElement.GetProperty("value").EnumerateArray());
    }

    [SkippableFact]
    public async Task Search_FindsByEveryWordOfTheNameOrPhone()
    {
        await using var scenario = await CreateAsync();
        using var site = scenario.Factory.CreateClient();
        await PlaceAsync(site, "Ірина", "+380954315238");
        await PlaceAsync(site, "Петро", "+380671112233");
        using var admin = scenario.Factory.CreateClientWithRole("Admin");

        using var byName = await JsonOf(await admin.GetAsync("/api/admin/quick-orders?search=ірин"));
        using var byPhone = await JsonOf(await admin.GetAsync("/api/admin/quick-orders?search=4315238"));
        using var both = await JsonOf(await admin.GetAsync("/api/admin/quick-orders?search=Петро%2011122"));

        Assert.Equal("Ірина", Assert.Single(byName.RootElement.GetProperty("value").EnumerateArray()).GetProperty("firstName").GetString());
        Assert.Equal("Ірина", Assert.Single(byPhone.RootElement.GetProperty("value").EnumerateArray()).GetProperty("firstName").GetString());
        Assert.Equal("Петро", Assert.Single(both.RootElement.GetProperty("value").EnumerateArray()).GetProperty("firstName").GetString());
    }

    [SkippableFact]
    public async Task Search_FindsByTheOrderNumberOrTheWholeId()
    {
        await using var scenario = await CreateAsync();
        using var site = scenario.Factory.CreateClient();
        // The site sends a random key per order, and the key becomes the id, so the numbers differ.
        var irynaId = await PlaceAsync(site, "Ірина", "+380954315238", Guid.NewGuid());
        await PlaceAsync(site, "Петро", "+380671112233", Guid.NewGuid());
        using var admin = scenario.Factory.CreateClientWithRole("Manager");
        var number = irynaId.ToString("N")[..8].ToUpperInvariant();

        using var byNumber = await JsonOf(await admin.GetAsync($"/api/admin/quick-orders?search={number}"));
        using var bySign = await JsonOf(await admin.GetAsync($"/api/admin/quick-orders?search=%E2%84%96{number.ToLowerInvariant()}"));
        using var byId = await JsonOf(await admin.GetAsync($"/api/admin/quick-orders?search={irynaId}"));

        foreach (var found in new[] { byNumber, bySign, byId })
            Assert.Equal(irynaId, Assert.Single(found.RootElement.GetProperty("value").EnumerateArray()).GetProperty("orderId").GetGuid());
    }

    [SkippableFact]
    public async Task UnknownOrder_IsNotFound_AndABadFilterIs400()
    {
        await using var scenario = await CreateAsync();
        using var admin = scenario.Factory.CreateClientWithRole("Manager");

        using var detail = await admin.GetAsync($"/api/admin/quick-orders/{Guid.NewGuid()}");
        using var resend = await admin.PostAsync($"/api/admin/quick-orders/{Guid.NewGuid()}/telegram-resend", null);
        using var filter = await admin.GetAsync("/api/admin/quick-orders?delivery=all");

        Assert.Equal(HttpStatusCode.NotFound, detail.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, resend.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, filter.StatusCode);
    }

    [SkippableFact]
    public async Task AdminEndpoints_NeedTheOrdersPolicy()
    {
        await using var scenario = await CreateAsync();
        var id = Guid.NewGuid();
        using var anonymous = scenario.Factory.CreateClient();
        using var user = scenario.Factory.CreateClientWithRole("User");

        foreach (var client in new[] { anonymous, user })
        {
            var expected = client == anonymous ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden;
            using var list = await client.GetAsync("/api/admin/quick-orders");
            using var detail = await client.GetAsync($"/api/admin/quick-orders/{id}");
            using var resend = await client.PostAsync($"/api/admin/quick-orders/{id}/telegram-resend", null);
            Assert.Equal(expected, list.StatusCode);
            Assert.Equal(expected, detail.StatusCode);
            Assert.Equal(expected, resend.StatusCode);
        }
    }

    private static async Task<Guid> PlaceAsync(HttpClient site, string firstName, string phone, Guid? idempotencyKey = null)
    {
        var json = $$"""{"firstName":"{{firstName}}","phone":"{{phone}}","lines":[{"productId":"101","title":"Ручка Кедр","unitPrice":125.50,"quantity":2}]}""";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        if (idempotencyKey is { } key)
            request.Headers.Add("Idempotency-Key", key.ToString());
        using var response = await site.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = await JsonOf(response);
        return body.RootElement.GetProperty("orderId").GetGuid();
    }

    private static async Task<JsonDocument> JsonOf(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
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
        var telegram = new FakeTelegramApi();
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:Default", postgres.ConnectionString);
            builder.UseSetting("Telegram:ChatId", "-100123");
            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(clock);
                services.RemoveAll<ITelegramBotClient>();
                services.AddSingleton<ITelegramBotClient>(
                    new TelegramBotClient(new TelegramBotClientOptions("123456:test-token"), new HttpClient(telegram)));
            });
        });

        return new Scenario(factory, clock, telegram);
    }

    /// <summary>Answers the Telegram Bot API: a sent document, or the refusal Telegram gives when the bot left the chat.</summary>
    private sealed class FakeTelegramApi : HttpMessageHandler
    {
        private int _delivered;

        public string? Refuse { get; set; }

        public int Delivered => _delivered;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = Refuse is null
                ? """{"ok":true,"result":{"message_id":1,"date":1764000000,"chat":{"id":-100123,"type":"supergroup"}}}"""
                : $$"""{"ok":false,"error_code":403,"description":"{{Refuse}}"}""";
            if (Refuse is null)
                Interlocked.Increment(ref _delivered);
            return Task.FromResult(new HttpResponseMessage(Refuse is null ? HttpStatusCode.OK : HttpStatusCode.Forbidden)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class ManualClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    private sealed class Scenario(WebApplicationFactory<Program> factory, ManualClock clock, FakeTelegramApi telegram)
        : IAsyncDisposable
    {
        public WebApplicationFactory<Program> Factory { get; } = factory;

        public FakeTelegramApi Telegram { get; } = telegram;

        public Task DispatchAsync() =>
            Factory.Services.GetRequiredService<IOutboxDispatcher>().DispatchDueAsync(CancellationToken.None);

        /// <summary>Runs the outbox until it gives up: the attempt after the commit was the first of ten.</summary>
        public async Task ExhaustRetriesAsync()
        {
            for (var attempt = 2; attempt <= 10; attempt++)
            {
                clock.Advance(TimeSpan.FromHours(2));
                await DispatchAsync();
            }
        }

        public ValueTask DisposeAsync() => Factory.DisposeAsync();
    }
}
