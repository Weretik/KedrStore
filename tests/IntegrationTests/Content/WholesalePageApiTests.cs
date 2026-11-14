using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Content.Application.Abstractions.Persistence;
using Content.Domain.Wholesale;
using Content.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Content;

/// <summary>The page «Співпраця» over HTTP on an in-memory content store: public read, admin access, validation.</summary>
public sealed class WholesalePageApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 0, 0, 0, TimeSpan.Zero);

    private static object Text(string language, string heroTitle, int steps = 3) => new
    {
        language,
        heroTitle,
        heroLead = "Lead",
        contactCardTitle = "Card",
        contactCardNote = "Note",
        assortmentTitle = "Assortment",
        assortmentIntro = "Intro",
        companyTitle = "company",
        companyText = "Company text",
        selectionTitle = "Selection",
        selectionText = "selection",
        warehouseTitle = "Warehouse",
        warehouseText = "warehouse",
        stepsTitle = "Steps",
        steps = Enumerable.Range(1, steps).Select(i => new { title = $"Step {i}", text = $"Text {i}" }).ToArray(),
        faqTitle = "Faq",
        faq = new[] { new { question = "Q?", answer = "A." } },
        closingTitle = "Closing",
    };

    [Fact]
    public async Task Public_ReadsOneLanguageWithoutSignIn_AndRefusesAnUnknownOne()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var json = JsonDocument.Parse(await client.GetStringAsync("/api/content/ru/wholesale"));
        using var unknown = await client.GetAsync("/api/content/en/wholesale");

        Assert.Equal("ru hero", json.RootElement.GetProperty("text").GetProperty("heroTitle").GetString());
        Assert.Equal(3, json.RootElement.GetProperty("text").GetProperty("steps").GetArrayLength());
        Assert.False(json.RootElement.TryGetProperty("updatedBy", out _), "the site never sees who edited");
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task AdminUpdate_IsForManagersAndAdmins(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.PutAsJsonAsync("/api/admin/content/wholesale",
            new { texts = new[] { Text("uk", "Нове"), Text("ru", "Новое") } });

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Manager_SavesBothLanguages_AndTheSiteReadsTheNewTitle()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var update = await manager.PutAsJsonAsync("/api/admin/content/wholesale",
            new { texts = new[] { Text("ru", "Опт и розница"), Text("uk", "Опт і роздріб") } });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        using var saved = JsonDocument.Parse(await update.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrEmpty(saved.RootElement.GetProperty("updatedBy").GetString()));
        Assert.Equal("uk", saved.RootElement.GetProperty("texts")[0].GetProperty("language").GetString());

        using var client = factory.CreateClient();
        using var json = JsonDocument.Parse(await client.GetStringAsync("/api/content/uk/wholesale"));
        Assert.Equal("Опт і роздріб", json.RootElement.GetProperty("text").GetProperty("heroTitle").GetString());
    }

    [Fact]
    public async Task WrongStepCountOrOneLanguage_IsAFieldError_AndNothingChanges()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var steps = await manager.PutAsJsonAsync("/api/admin/content/wholesale",
            new { texts = new[] { Text("uk", "Нове", steps: 2), Text("ru", "Новое") } });
        using var oneLanguage = await manager.PutAsJsonAsync("/api/admin/content/wholesale",
            new { texts = new[] { Text("uk", "Нове") } });

        Assert.Equal(HttpStatusCode.BadRequest, steps.StatusCode);
        Assert.Contains("steps", (await steps.Content.ReadAsStringAsync()).ToLowerInvariant());
        Assert.Equal(HttpStatusCode.BadRequest, oneLanguage.StatusCode);
        Assert.Equal("uk hero", (await db.WholesalePages.AsNoTracking().SingleAsync()).Uk.HeroTitle);
    }

    private static ContentDbContext Seed()
    {
        var db = new ContentDbContext(new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.WholesalePages.Add(WholesalePage.Create(Create("uk"), Create("ru"), Now));
        db.SaveChanges();
        return db;
    }

    private static WholesalePageText Create(string language) => WholesalePageText.Create(new(
        $"{language} hero", "Lead", "Card", "Note", "Assortment", "Intro", "company", "Company text", "Selection",
        "selection", "Warehouse", "warehouse", "Steps", [("1", "a"), ("2", "b"), ("3", "c")], "Faq",
        [("Q?", "A.")], "Closing")).Value;

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
