using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Content.Application.Abstractions.Media;
using Content.Application.Abstractions.Persistence;
using Content.Domain.Articles;
using Content.Infrastructure.Persistence;
using IntegrationTests.Identity.Avatars;
using IntegrationTests.TestSupport.Api;
using IntegrationTests.TestSupport.Content;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Content;

/// <summary>Article endpoints over HTTP on an in-memory content store: access, JSON shape, cover caching.</summary>
public sealed class ArticlesApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task AdminList_IsForManagersAndAdmins(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.GetAsync("/api/admin/articles");

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Public_ListsPublishedOnly_WithRelativeCoverPath()
    {
        await using var db = Seed();
        var id = db.Articles.Single(article => article.Slug == "petli").Id.Value;
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var list = JsonDocument.Parse(await client.GetStringAsync("/api/content/uk/articles"));
        var item = Assert.Single(list.RootElement.EnumerateArray());
        Assert.Equal("petli", item.GetProperty("slug").GetString());
        Assert.Equal("2026-11-04", item.GetProperty("publishedOn").GetString());
        Assert.Equal($"/api/content/articles/{id}/cover?v=1", item.GetProperty("coverPath").GetString());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("coverUrl").ValueKind);

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/content/uk/articles/chernetka")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/content/en/articles")).StatusCode);

        using var cover = await client.GetAsync($"/api/content/articles/{id}/cover?v=1");
        Assert.Equal("image/webp", cover.Content.Headers.ContentType?.MediaType);
        Assert.Contains("immutable", cover.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Manager_UploadsCoverFirst_ThenCreatesArticleWithIt()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(TestImages.Create(1200, 600));
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(file, "file", "cover.jpg");
        using var upload = await manager.PostAsync("/api/admin/articles/covers", form);
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        using var uploaded = JsonDocument.Parse(await upload.Content.ReadAsStringAsync());
        var mediaFileId = uploaded.RootElement.GetProperty("mediaFileId").GetInt64();
        Assert.Equal("https://cdn.example.test/articles/cover.webp", uploaded.RootElement.GetProperty("url").GetString());

        using var create = await manager.PostAsJsonAsync("/api/admin/articles", new
        {
            publishedOn = "2026-11-05",
            isPublished = true,
            coverMediaFileId = mediaFileId,
            texts = new[]
            {
                new { language = "uk", title = "Нова стаття", excerpt = "Коротко.", bodyHtml = "<p>Текст</p>" },
                new { language = "ru", title = "Новая статья", excerpt = "Коротко.", bodyHtml = "<p>Текст</p>" },
            }
        });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        using var created = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        Assert.Equal("nova-stattia", created.RootElement.GetProperty("slug").GetString());
        Assert.Equal(mediaFileId, created.RootElement.GetProperty("coverMediaFileId").GetInt64());
        Assert.Equal("https://cdn.example.test/articles/cover.webp", created.RootElement.GetProperty("coverUrl").GetString());
        Assert.Equal(JsonValueKind.Null, created.RootElement.GetProperty("coverPath").ValueKind);

        using var invalid = await manager.PostAsJsonAsync("/api/admin/articles", new { publishedOn = "2026-11-05", texts = Array.Empty<object>() });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    private static ContentDbContext Seed()
    {
        var db = new ContentDbContext(new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var published = Article.Create("petli", new DateOnly(2026, 11, 4), true, Texts("Петлі"), Now, "seed").Value;
        var draft = Article.Create("chernetka", new DateOnly(2026, 11, 4), false, Texts("Чернетка"), Now, "seed").Value;
        db.Articles.Add(published);
        db.SaveChanges();
        db.Articles.Add(draft);
        db.SaveChanges();
        db.Entry(published).Property(article => article.CoverVersion).CurrentValue = 1;
        db.ArticleCovers.Add(ArticleCover.Create(published.Id, [1, 2, 3], "image/webp"));
        db.SaveChanges();
        return db;
    }

    private static ArticleText[] Texts(string title) =>
        [new("uk", title, "Коротко.", "<p>Текст</p>"), new("ru", title, "Коротко.", "<p>Текст</p>")];

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
                var media = new FakeArticleMediaLibrary();
                services.RemoveAll<IMediaImageReader>();
                services.RemoveAll<IMediaImageWriter>();
                services.AddSingleton<IMediaImageReader>(media);
                services.AddSingleton<IMediaImageWriter>(media);
            });
        });
}
