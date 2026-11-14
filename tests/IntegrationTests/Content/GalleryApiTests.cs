using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Content.Application.Abstractions.Integrations;
using Content.Application.Abstractions.Media;
using Content.Application.Abstractions.Persistence;
using Content.Domain.Gallery;
using Content.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Content;

/// <summary>The gallery over HTTP on an in-memory store with a fake media library and YouTube.</summary>
public sealed class GalleryApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);
    private const string Base = "/api/admin/content/gallery";

    [Fact]
    public async Task Public_ShowsVideosAndPhotosInOrder_AndSkipsADeletedFile()
    {
        await using var db = Seed();
        db.GalleryItems.Add(GalleryItem.CreatePhoto(7, 1, GalleryItemTexts.Create("Фото", "Фото ру", null, null), Now));
        db.GalleryItems.Add(GalleryItem.CreatePhoto(999, 2, GalleryItemTexts.Empty, Now));
        await db.SaveChangesAsync();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var json = JsonDocument.Parse(await client.GetStringAsync("/api/content/ru/gallery"));
        using var badLanguage = await client.GetAsync("/api/content/en/gallery");

        var items = json.RootElement.EnumerateArray().ToList();
        Assert.Equal(2, items.Count);
        Assert.Equal("video", items[0].GetProperty("kind").GetString());
        Assert.Equal("Петля скрытая", items[0].GetProperty("title").GetString());
        Assert.Equal("https://i.ytimg.com/vi/0FDRbSaQ9Ec/hqdefault.jpg", items[0].GetProperty("imageUrl").GetString());
        Assert.Equal("photo", items[1].GetProperty("kind").GetString());
        Assert.Equal("Фото ру", items[1].GetProperty("title").GetString());
        Assert.Equal("https://images-kedr.cdn.express/gallery/7.jpg", items[1].GetProperty("imageUrl").GetString());
        Assert.Equal(1200, items[1].GetProperty("width").GetInt32());
        Assert.Equal(HttpStatusCode.BadRequest, badLanguage.StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task Admin_IsForContentManagers(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.GetAsync(Base);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Manager_PastesALink_AddsTheVideoAndPhotos_ReordersEditsAndRemoves()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var preview = JsonDocument.Parse(await manager.GetStringAsync(
            $"{Base}/youtube-preview?url={Uri.EscapeDataString("https://youtu.be/j9wrjRub9xI?si=x")}"));
        Assert.Equal("j9wrjRub9xI", preview.RootElement.GetProperty("youtubeId").GetString());
        Assert.Equal("Video j9wrjRub9xI", preview.RootElement.GetProperty("title").GetString());
        Assert.False(preview.RootElement.GetProperty("alreadyInGallery").GetBoolean());

        using var video = await manager.PostAsJsonAsync($"{Base}/videos",
            new { url = "https://www.youtube.com/watch?v=j9wrjRub9xI", titleUk = "Порівняння", titleRu = "Сравнение" });
        Assert.Equal(HttpStatusCode.OK, video.StatusCode);
        using var again = await manager.PostAsJsonAsync($"{Base}/videos", new { url = "j9wrjRub9xI" });
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        using var notYoutube = await manager.PostAsJsonAsync($"{Base}/videos", new { url = "https://vimeo.com/1" });
        Assert.Equal(HttpStatusCode.BadRequest, notYoutube.StatusCode);

        using var photos = await manager.PostAsJsonAsync($"{Base}/photos", new { mediaFileIds = new[] { 7L, 8L, 7L } });
        Assert.Equal(HttpStatusCode.OK, photos.StatusCode);
        using var unknown = await manager.PostAsJsonAsync($"{Base}/photos", new { mediaFileIds = new[] { 999L } });
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        using var twice = await manager.PostAsJsonAsync($"{Base}/photos", new { mediaFileIds = new[] { 8L } });
        var afterAdd = await ReadList(twice);
        Assert.Equal(["video", "video", "photo", "photo"], afterAdd.Select(item => item.GetProperty("kind").GetString()));
        Assert.Equal("7.jpg", afterAdd[2].GetProperty("fileName").GetString());

        var ids = afterAdd.Select(item => item.GetProperty("id").GetInt64()).ToArray();
        using var stale = await manager.PutAsJsonAsync($"{Base}/order", new { ids = ids[..3] });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        using var reordered = await manager.PutAsJsonAsync($"{Base}/order", new { ids = ids.Reverse().ToArray() });
        var afterOrder = await ReadList(reordered);
        Assert.Equal(ids.Reverse(), afterOrder.Select(item => item.GetProperty("id").GetInt64()));

        using var edited = await manager.PutAsJsonAsync($"{Base}/items/{ids[2]}",
            new { titleUk = " Склад ", titleRu = "Склад", captionUk = "Київ", captionRu = (string?)null });
        var afterEdit = await ReadList(edited);
        var photo = afterEdit.Single(item => item.GetProperty("id").GetInt64() == ids[2]);
        Assert.Equal("Склад", photo.GetProperty("titleUk").GetString());
        Assert.Equal("Київ", photo.GetProperty("captionUk").GetString());

        using var deleted = await manager.DeleteAsync($"{Base}/items/{ids[0]}");
        Assert.Equal(3, (await ReadList(deleted)).Count);
        using var missing = await manager.DeleteAsync($"{Base}/items/{ids[0]}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    private static async Task<List<JsonElement>> ReadList(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.EnumerateArray().Select(item => item.Clone()).ToList();
    }

    private static ContentDbContext Seed()
    {
        var db = new ContentDbContext(new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.GalleryItems.Add(GalleryItem.CreateVideo("0FDRbSaQ9Ec", 0,
            GalleryItemTexts.Create("Прихована петля", "Петля скрытая", null, null), Now));
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
                services.RemoveAll<IYoutubeVideoInfo>();
                services.AddSingleton<IYoutubeVideoInfo, FakeYoutube>();
            });
        });

    private sealed class FakeMediaLibrary : IMediaImageReader
    {
        public Task<IReadOnlyDictionary<long, MediaImage>> GetImagesAsync(IReadOnlyCollection<long> mediaFileIds,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyDictionary<long, MediaImage>>(mediaFileIds
                .Where(id => id is 7 or 8).Distinct()
                .ToDictionary(id => id, id => new MediaImage(
                    $"https://images-kedr.cdn.express/gallery/{id}.jpg", $"{id}.jpg", 1200, 800)));
    }

    private sealed class FakeYoutube : IYoutubeVideoInfo
    {
        public Task<string?> GetTitleAsync(string youtubeId, CancellationToken cancellationToken)
            => Task.FromResult<string?>($"Video {youtubeId}");
    }
}
