using System.Net;
using System.Text.Json;
using Content.Application.Abstractions.Persistence;
using Content.Domain.Articles;
using Content.Domain.Documents;
using Content.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Content;

/// <summary>Content block of the admin «Огляд» over HTTP on an in-memory content store.</summary>
public sealed class ContentOverviewApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Overview_CountsDraftsLegacyCoversAndDocumentDrafts_AndNamesTheOldestDraft()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var json = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/content/overview"));

        var root = json.RootElement;
        Assert.Equal(2, root.GetProperty("articleDrafts").GetInt32());
        Assert.Equal("Стара чернетка", root.GetProperty("oldestArticleDraft").GetProperty("title").GetString());
        Assert.Equal(1, root.GetProperty("legacyArticleCovers").GetInt32());
        Assert.Equal(1, root.GetProperty("documentDrafts").GetInt32());
    }

    [Fact]
    public async Task Overview_WithoutContent_HasZeroCountsAndNoDraft()
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var json = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/content/overview"));

        Assert.Equal(0, json.RootElement.GetProperty("articleDrafts").GetInt32());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("oldestArticleDraft").ValueKind);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task Overview_IsForManagersAndAdmins(string? role, HttpStatusCode expected)
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.GetAsync("/api/admin/content/overview");

        Assert.Equal(expected, response.StatusCode);
    }

    private static ContentDbContext Seed()
    {
        var db = CreateDb();
        var published = Article.Create("petli", new DateOnly(2026, 11, 4), true, Texts("Петлі"), Now, "seed").Value;
        db.Articles.Add(published);
        db.Articles.Add(Article.Create("nova", new DateOnly(2026, 11, 5), false, Texts("Нова чернетка"), Now, "seed").Value);
        db.Articles.Add(Article.Create("stara", new DateOnly(2026, 10, 1), false, Texts("Стара чернетка"), Now.AddDays(-12), "seed").Value);
        db.SaveChanges();
        db.Entry(published).Property(article => article.CoverVersion).CurrentValue = 1;

        var offer = LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer, DocumentTexts(), Now, "seed").Value;
        offer.Publish(1, new DateOnly(2026, 11, 3), Now, "seed");
        db.LegalDocumentVersions.Add(offer);
        db.LegalDocumentVersions.Add(LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer, DocumentTexts(), Now, "seed").Value);
        db.SaveChanges();
        return db;
    }

    private static ArticleText[] Texts(string title) =>
        [new("uk", title, "Коротко.", "<p>Текст</p>"), new("ru", title, "Коротко.", "<p>Текст</p>")];

    private static LegalDocumentTextInput[] DocumentTexts() =>
    [
        new("uk", "Публічна оферта", "<h2>Загальні</h2><p>Текст</p>"),
        new("ru", "Публичная оферта", "<h2>Общие</h2><p>Текст</p>"),
    ];

    private static ContentDbContext CreateDb() => new(new DbContextOptionsBuilder<ContentDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

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
