using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Content.Application.Abstractions.Persistence;
using Content.Domain.Seo;
using Content.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace IntegrationTests.Content.Seo;

/// <summary>Search texts of the site pages over HTTP on an in-memory content store.</summary>
public sealed class PageSeoApiTests
{
    [Fact]
    public async Task Admin_ListsEveryPageInMenuOrder_EmptyUntilSaved()
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var json = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/content/page-seo"));

        var pages = json.RootElement.EnumerateArray().ToList();
        Assert.Equal(SitePages.All, pages.Select(page => page.GetProperty("page").GetString()));
        Assert.All(pages, page => Assert.Equal(JsonValueKind.Null, page.GetProperty("titleUk").ValueKind));
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task AdminUpdate_IsForManagersAndAdmins(string? role, HttpStatusCode expected)
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.PutAsJsonAsync("/api/admin/content/page-seo/Wholesale",
            new { titleUk = "Фурнітура оптом" });

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Save_CreatesThenReplaces_AndTheSiteGetsOnlySetValuesInItsLanguage()
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var first = await manager.PutAsJsonAsync("/api/admin/content/page-seo/Wholesale",
            new { titleUk = "  Фурнітура оптом  ", descriptionUk = "Опис", titleRu = "Фурнитура оптом" });
        using var second = await manager.PutAsJsonAsync("/api/admin/content/page-seo/Wholesale",
            new { titleUk = "Дверна фурнітура оптом", titleRu = "" });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        using var saved = JsonDocument.Parse(await second.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrEmpty(saved.RootElement.GetProperty("updatedBy").GetString()));
        Assert.Equal(JsonValueKind.Null, saved.RootElement.GetProperty("descriptionUk").ValueKind);

        using var client = factory.CreateClient();
        using var uk = JsonDocument.Parse(await client.GetStringAsync("/api/content/uk/page-seo"));
        using var ru = JsonDocument.Parse(await client.GetStringAsync("/api/content/ru/page-seo"));

        var page = Assert.Single(uk.RootElement.EnumerateArray());
        Assert.Equal("Wholesale", page.GetProperty("page").GetString());
        Assert.Equal("Дверна фурнітура оптом", page.GetProperty("title").GetString());
        Assert.False(page.TryGetProperty("updatedBy", out _), "the site never sees who edited");
        Assert.Empty(ru.RootElement.EnumerateArray());
        Assert.Equal(1, await db.PageSeo.CountAsync());
    }

    [Fact]
    public async Task UnknownPageLanguageOrTooLongText_IsRefused()
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var page = await manager.PutAsJsonAsync("/api/admin/content/page-seo/Checkout", new { titleUk = "x" });
        using var tooLong = await manager.PutAsJsonAsync("/api/admin/content/page-seo/Home",
            new { descriptionRu = new string('a', PageSeo.DescriptionMaxLength + 1) });
        using var language = await factory.CreateClient().GetAsync("/api/content/en/page-seo");

        Assert.Equal(HttpStatusCode.BadRequest, page.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        Assert.Contains("descriptionRu", await tooLong.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.BadRequest, language.StatusCode);
        Assert.Equal(0, await db.PageSeo.CountAsync());
    }

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
