using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Content.Application.Abstractions.Media;
using Content.Application.Abstractions.Persistence;
using Content.Domain.Home;
using Content.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace IntegrationTests.Content.Pages;

/// <summary>Home page FAQ and brands over HTTP on an in-memory store with a fake media library.</summary>
public sealed class HomeBlocksApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);
    private const string Base = "/api/admin/content/home";

    [Fact]
    public async Task Public_ShowsTheLanguageInOrder_AndHidesABrandWhoseLogoWasDeleted()
    {
        await using var db = Seed();
        db.HomeBrands.Add(HomeBrand.Create("Leador", 8, 1, Now).Value);
        db.HomeBrands.Add(HomeBrand.Create("Gone", 999, 2, Now).Value);
        await db.SaveChangesAsync();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var json = JsonDocument.Parse(await client.GetStringAsync("/api/content/ru/home"));
        using var badLanguage = await client.GetAsync("/api/content/en/home");

        var faq = json.RootElement.GetProperty("faq").EnumerateArray().ToList();
        Assert.Equal(["Вопрос 1", "Вопрос 2"], faq.Select(item => item.GetProperty("question").GetString()));
        Assert.Equal("Ответ 1", faq[0].GetProperty("answer").GetString());
        var brands = json.RootElement.GetProperty("brands").EnumerateArray().ToList();
        Assert.Equal(["Korfad", "Leador"], brands.Select(brand => brand.GetProperty("name").GetString()));
        Assert.Equal("https://images-kedr.cdn.express/brands/7.webp", brands[0].GetProperty("logoUrl").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, badLanguage.StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    public async Task Admin_IsForContentManagers(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.GetAsync(Base);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Manager_EditsTheFaq_AddsReordersAndRemoves()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var created = await manager.PostAsJsonAsync($"{Base}/faq",
            new { questionUk = "Питання 3", questionRu = "Вопрос 3", answerUk = "Відповідь 3", answerRu = "Ответ 3" });
        var faq = (await Read(created)).GetProperty("faq").EnumerateArray().ToList();
        Assert.Equal("Питання 3", faq[^1].GetProperty("questionUk").GetString());
        using var empty = await manager.PostAsJsonAsync($"{Base}/faq", new { questionUk = "Питання" });
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);

        var ids = faq.Select(item => item.GetProperty("id").GetInt64()).ToArray();
        using var stale = await manager.PutAsJsonAsync($"{Base}/faq/order", new { ids = ids[..2] });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        using var reordered = await manager.PutAsJsonAsync($"{Base}/faq/order", new { ids = ids.Reverse().ToArray() });
        Assert.Equal(ids.Reverse(), (await Read(reordered)).GetProperty("faq").EnumerateArray()
            .Select(item => item.GetProperty("id").GetInt64()));

        using var edited = await manager.PutAsJsonAsync($"{Base}/faq/{ids[0]}",
            new { questionUk = "Нове", questionRu = "Новое", answerUk = "Так", answerRu = "Да" });
        Assert.Contains((await Read(edited)).GetProperty("faq").EnumerateArray(),
            item => item.GetProperty("questionUk").GetString() == "Нове");

        using var deleted = await manager.DeleteAsync($"{Base}/faq/{ids[0]}");
        Assert.Equal(2, (await Read(deleted)).GetProperty("faq").GetArrayLength());
        using var missing = await manager.DeleteAsync($"{Base}/faq/{ids[0]}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Manager_AddsBrandsWithLibraryLogos_ChangesAndReorders()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var unknownLogo = await manager.PostAsJsonAsync($"{Base}/brands", new { name = "SOVA", logoMediaFileId = 999 });
        Assert.Equal(HttpStatusCode.BadRequest, unknownLogo.StatusCode);
        using var created = await manager.PostAsJsonAsync($"{Base}/brands", new { name = "SOVA", logoMediaFileId = 8 });
        var brands = (await Read(created)).GetProperty("brands").EnumerateArray().ToList();
        Assert.Equal(["Korfad", "SOVA"], brands.Select(brand => brand.GetProperty("name").GetString()));
        Assert.Equal("8.webp", brands[1].GetProperty("logoFileName").GetString());
        Assert.False(brands[1].GetProperty("logoMissing").GetBoolean());

        var ids = brands.Select(brand => brand.GetProperty("id").GetInt64()).ToArray();
        using var renamed = await manager.PutAsJsonAsync($"{Base}/brands/{ids[1]}", new { name = "Sova", logoMediaFileId = 7 });
        Assert.Equal("Sova", (await Read(renamed)).GetProperty("brands")[1].GetProperty("name").GetString());
        using var reordered = await manager.PutAsJsonAsync($"{Base}/brands/order", new { ids = ids.Reverse().ToArray() });
        Assert.Equal("Sova", (await Read(reordered)).GetProperty("brands")[0].GetProperty("name").GetString());
        using var deleted = await manager.DeleteAsync($"{Base}/brands/{ids[0]}");
        Assert.Equal(1, (await Read(deleted)).GetProperty("brands").GetArrayLength());
    }

    private static async Task<JsonElement> Read(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.Clone();
    }

    private static ContentDbContext Seed()
    {
        var db = new ContentDbContext(new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.HomeFaqItems.Add(HomeFaqItem.Create(new HomeFaqTexts("Питання 2", "Вопрос 2", "Відповідь 2", "Ответ 2"), 1, Now).Value);
        db.HomeFaqItems.Add(HomeFaqItem.Create(new HomeFaqTexts("Питання 1", "Вопрос 1", "Відповідь 1", "Ответ 1"), 0, Now).Value);
        db.HomeBrands.Add(HomeBrand.Create("Korfad", 7, 0, Now).Value);
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
                services.RemoveAll<IMediaImageReader>();
                services.AddSingleton<IMediaImageReader, FakeMediaLibrary>();
            });
        });

    private sealed class FakeMediaLibrary : IMediaImageReader
    {
        public Task<IReadOnlyDictionary<long, MediaImage>> GetImagesAsync(IReadOnlyCollection<long> mediaFileIds,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyDictionary<long, MediaImage>>(mediaFileIds
                .Where(id => id is 7 or 8).Distinct()
                .ToDictionary(id => id, id => new MediaImage(
                    $"https://images-kedr.cdn.express/brands/{id}.webp", $"{id}.webp", 320, 128)));
    }
}
