using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Content.Application.Abstractions.Persistence;
using Content.Domain.Documents;
using Content.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Content;

/// <summary>Site documents over HTTP on an in-memory store: public read, draft, publish, history.</summary>
public sealed class LegalDocumentsApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);
    private const string Base = "/api/admin/content/documents";

    private static object Texts(string ukBody) => new
    {
        texts = new[]
        {
            new { language = "uk", title = "Публічна оферта", bodyHtml = ukBody },
            new { language = "ru", title = "Публичная оферта", bodyHtml = "<h2>Общие</h2><p>Текст</p>" },
        },
    };

    [Fact]
    public async Task Public_ReadsThePublishedEdition_AndUnknownKindIs404()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var json = JsonDocument.Parse(await client.GetStringAsync("/api/content/uk/documents/public-offer"));
        using var missing = await client.GetAsync("/api/content/uk/documents/cookies");
        using var badLanguage = await client.GetAsync("/api/content/en/documents/public-offer");

        Assert.Equal("public-offer", json.RootElement.GetProperty("kind").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("number").GetInt32());
        Assert.Equal("2026-11-03", json.RootElement.GetProperty("effectiveFrom").GetString());
        Assert.Equal("Публічна оферта", json.RootElement.GetProperty("title").GetString());
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, badLanguage.StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task AdminList_IsForContentManagers(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.GetAsync(Base);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Manager_EditsADraft_PublishesIt_AndTheSiteShowsTheNewEdition()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");
        using var site = factory.CreateClient();

        using var created = await manager.PostAsync($"{Base}/public-offer/versions", null);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        using var draft = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = draft.RootElement.GetProperty("id").GetInt64();
        Assert.Equal("draft", draft.RootElement.GetProperty("status").GetString());
        Assert.Contains("Старий текст", draft.RootElement.GetProperty("texts")[1].GetProperty("bodyHtml").GetString());

        using var second = await manager.PostAsync($"{Base}/public-offer/versions", null);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        using var saved = await manager.PutAsJsonAsync($"{Base}/versions/{id}",
            Texts("<h2>Загальні</h2><p>Новий текст</p><script>alert(1)</script>"));
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        Assert.DoesNotContain("script", await saved.Content.ReadAsStringAsync());

        using var tooEarly = await manager.PostAsJsonAsync($"{Base}/versions/{id}/publish",
            new { effectiveFrom = "2026-11-01" });
        Assert.Equal(HttpStatusCode.BadRequest, tooEarly.StatusCode);
        Assert.Contains("effectiveFrom", await tooEarly.Content.ReadAsStringAsync());

        using var published = await manager.PostAsJsonAsync($"{Base}/versions/{id}/publish",
            new { effectiveFrom = "2026-11-10" });
        Assert.Equal(HttpStatusCode.OK, published.StatusCode);

        using var onSite = JsonDocument.Parse(await site.GetStringAsync("/api/content/uk/documents/public-offer"));
        Assert.Equal(2, onSite.RootElement.GetProperty("number").GetInt32());
        Assert.Equal("2026-11-10", onSite.RootElement.GetProperty("effectiveFrom").GetString());
        Assert.Contains("Новий текст", onSite.RootElement.GetProperty("bodyHtml").GetString());

        using var history = JsonDocument.Parse(await manager.GetStringAsync($"{Base}/public-offer/versions"));
        Assert.Equal(new string?[] { "published", "archived" },
            history.RootElement.EnumerateArray().Select(v => v.GetProperty("status").GetString()).ToArray());

        using var editOld = await manager.PutAsJsonAsync($"{Base}/versions/{id}", Texts("<p>Ще</p>"));
        Assert.Equal(HttpStatusCode.Conflict, editOld.StatusCode);
    }

    [Fact]
    public async Task DeleteDraft_RemovesOnlyDrafts()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");

        using var created = await admin.PostAsync($"{Base}/delivery-payment/versions", null);
        using var draft = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = draft.RootElement.GetProperty("id").GetInt64();
        var publishedId = db.LegalDocumentVersions.AsNoTracking()
            .Single(v => v.Kind == LegalDocumentKind.DeliveryPayment && v.Status == LegalDocumentStatus.Published).Id;

        using var deleted = await admin.DeleteAsync($"{Base}/versions/{id}");
        using var deletePublished = await admin.DeleteAsync($"{Base}/versions/{publishedId}");

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, deletePublished.StatusCode);
        using var list = JsonDocument.Parse(await admin.GetStringAsync(Base));
        var delivery = list.RootElement.EnumerateArray().Single(d => d.GetProperty("kind").GetString() == "delivery-payment");
        Assert.Equal(JsonValueKind.Null, delivery.GetProperty("draft").ValueKind);
    }

    private static ContentDbContext Seed()
    {
        var db = new ContentDbContext(new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        foreach (var kind in Enum.GetValues<LegalDocumentKind>())
        {
            var version = LegalDocumentVersion.CreateDraft(kind,
            [
                new LegalDocumentTextInput("uk", "Публічна оферта", "<h2>Загальні</h2><p>Старий текст</p>"),
                new LegalDocumentTextInput("ru", "Публичная оферта", "<h2>Общие</h2><p>Старый текст</p>"),
            ], Now, "seed");
            version.Publish(1, new DateOnly(2026, 11, 3), Now, "seed");
            db.LegalDocumentVersions.Add(version);
        }

        db.SaveChanges();
        return db;
    }

    private static WebApplicationFactory<Program> CreateFactory(ContentDbContext db)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
                services.RemoveAll<IReadContentDbContext>();
                services.RemoveAll<ContentDbContext>();
                services.AddSingleton(db);
                services.AddSingleton<IReadContentDbContext>(db);
            });
        });
}
