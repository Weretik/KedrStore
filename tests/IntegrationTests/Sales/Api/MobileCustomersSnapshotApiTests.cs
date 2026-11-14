using System.Net;
using System.Text.Json;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sales.Application.Contracts.Persistence;
using Sales.Domain.Customers.Entities;
using Sales.Infrastructure.DataBase;

namespace IntegrationTests.Sales.Api;

/// <summary>Customers snapshot for the offline manager app (spec platform/004, TS-002) on an in-memory Sales DB.</summary>
public sealed class MobileCustomersSnapshotApiTests
{
    private const string Route = "/api/mobile/sync/customers";
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Snapshot_HasActiveCustomersWithPriceRules_AndNoSettlementsYet()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var response = await manager.GetAsync(Route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        Assert.Equal(response.Headers.ETag!.Tag, $"\"{root.GetProperty("version").GetString()}\"");
        var customers = root.GetProperty("customers");
        Assert.Equal(["c-1", "c-2"], customers.EnumerateArray().Select(c => c.GetProperty("counterpartyId").GetString()));
        var first = customers[0];
        Assert.Equal("ТОВ Альфа", first.GetProperty("name").GetString());
        Assert.Equal(12, first.GetProperty("defaultPriceTypeId").GetInt32());
        Assert.Equal([(10, 14), (20, 15)], first.GetProperty("categoryPriceTypes").EnumerateArray()
            .Select(rule => (rule.GetProperty("categoryId").GetInt32(), rule.GetProperty("priceTypeId").GetInt32())));
        Assert.Equal(JsonValueKind.Null, first.GetProperty("debt").ValueKind);
        Assert.Equal(JsonValueKind.Null, first.GetProperty("balance").ValueKind);
        Assert.Empty(customers[1].GetProperty("categoryPriceTypes").EnumerateArray());
    }

    [Fact]
    public async Task Snapshot_AnswersNotModifiedForTheSameVersion_AndANewVersionAfterANewRule()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var first = await manager.GetAsync(Route);
        var etag = first.Headers.ETag!;

        using var again = new HttpRequestMessage(HttpMethod.Get, Route);
        again.Headers.IfNoneMatch.Add(etag);
        using var notModified = await manager.SendAsync(again);
        Assert.Equal(HttpStatusCode.NotModified, notModified.StatusCode);

        db.CounterpartyCategoryPriceTypes.Add(CounterpartyCategoryPriceType.Create("c-2", 30, 14).Value);
        db.SaveChanges();

        using var changed = new HttpRequestMessage(HttpMethod.Get, Route);
        changed.Headers.IfNoneMatch.Add(etag);
        using var updated = await manager.SendAsync(changed);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.NotEqual(etag, updated.Headers.ETag);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task Snapshot_IsForManagersAndAdmins(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.GetAsync(Route);

        Assert.Equal(expected, response.StatusCode);
    }

    private static SalesDbContext Seed()
    {
        var db = new SalesDbContext(new DbContextOptionsBuilder<SalesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        db.Counterparties.Add(Counterparty.Create("c-2", Guid.NewGuid(), "ФОП Бета", "b@example.com", null, 14, Now).Value);
        db.Counterparties.Add(Counterparty.Create("c-1", Guid.NewGuid(), "ТОВ Альфа", "a@example.com", "+380501112233", 12, Now).Value);
        var gone = Counterparty.Create("c-3", Guid.NewGuid(), "Видалений", "d@example.com", null, 12, Now).Value;
        gone.MarkAsDeleted(Now);
        db.Counterparties.Add(gone);
        db.CounterpartyCategoryPriceTypes.Add(CounterpartyCategoryPriceType.Create("c-1", 20, 15).Value);
        db.CounterpartyCategoryPriceTypes.Add(CounterpartyCategoryPriceType.Create("c-1", 10, 14).Value);
        db.CounterpartyCategoryPriceTypes.Add(CounterpartyCategoryPriceType.Create("c-3", 10, 14).Value);

        db.SaveChanges();
        return db;
    }

    private static WebApplicationFactory<Program> CreateFactory(SalesDbContext db)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
                services.RemoveAll<IReadSalesDbContext>();
                services.AddSingleton<IReadSalesDbContext>(db);
            });
        });
}
