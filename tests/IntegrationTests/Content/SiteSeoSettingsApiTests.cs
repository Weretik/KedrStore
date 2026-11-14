using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Content.Application.Abstractions.Media;
using Content.Application.Abstractions.Persistence;
using Content.Domain.Seo;
using Content.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Content;

/// <summary>Site SEO settings over HTTP on an in-memory content store: public read, admin access, validation.</summary>
public sealed class SiteSeoSettingsApiTests
{
    private const string AdminUrl = "/api/admin/content/seo-settings";
    private const string PublicUrl = "/api/content/seo-settings";

    private static readonly object ValidBody = new
    {
        googleAnalyticsId = "g-ab12cd34ef",
        googleTagManagerId = "GTM-AB12CD3",
        googleSiteVerification = """<meta name="google-site-verification" content="AbCdEf_123-xyz" />""",
        bingSiteVerification = "0123456789ABCDEF0123456789ABCDEF",
        shareImageMediaFileId = 7,
    };

    [Fact]
    public async Task Public_ReadsEmptySettingsWithoutSignIn()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var json = JsonDocument.Parse(await client.GetStringAsync(PublicUrl));

        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("googleAnalyticsId").ValueKind);
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("shareImage").ValueKind);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    [InlineData("Seo", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task AdminUpdate_IsForContentEditors(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.PutAsJsonAsync(AdminUrl, ValidBody);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Seo_SavesCodes_AndTheSiteReadsThemWithThePicture()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var seo = factory.CreateClientWithRole("Seo");

        using var update = await seo.PutAsJsonAsync(AdminUrl, ValidBody);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        using var saved = JsonDocument.Parse(await update.Content.ReadAsStringAsync());
        Assert.Equal("G-AB12CD34EF", saved.RootElement.GetProperty("googleAnalyticsId").GetString());
        Assert.Equal("AbCdEf_123-xyz", saved.RootElement.GetProperty("googleSiteVerification").GetString());
        Assert.Equal("7.jpg", saved.RootElement.GetProperty("shareImage").GetProperty("fileName").GetString());
        Assert.False(string.IsNullOrEmpty(saved.RootElement.GetProperty("updatedBy").GetString()));

        using var client = factory.CreateClient();
        using var json = JsonDocument.Parse(await client.GetStringAsync(PublicUrl));
        var root = json.RootElement;
        Assert.Equal("GTM-AB12CD3", root.GetProperty("googleTagManagerId").GetString());
        Assert.Equal("0123456789ABCDEF0123456789ABCDEF", root.GetProperty("bingSiteVerification").GetString());
        Assert.Equal("https://images-kedr.cdn.express/og/7.jpg", root.GetProperty("shareImage").GetProperty("url").GetString());
        Assert.Equal(1200, root.GetProperty("shareImage").GetProperty("width").GetInt32());
        Assert.False(root.TryGetProperty("updatedBy", out _), "the site never sees who edited");
    }

    [Fact]
    public async Task AFileNotInTheLibrary_IsAFieldError_AndNothingChanges()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var response = await manager.PutAsJsonAsync(AdminUrl, new { shareImageMediaFileId = 99 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("shareImageMediaFileId", await response.Content.ReadAsStringAsync());
        Assert.Null((await db.SiteSeoSettings.AsNoTracking().SingleAsync()).ShareImageMediaFileId);
    }

    [Fact]
    public async Task AnInvalidId_IsAFieldError()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var response = await manager.PutAsJsonAsync(AdminUrl, new { googleAnalyticsId = "UA-12345-1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("googleAnalyticsId", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ADeletedPicture_IsMarkedMissingInTheAdmin_AndHiddenFromTheSite()
    {
        await using var db = Seed(shareImageMediaFileId: 99);
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var admin = JsonDocument.Parse(await manager.GetStringAsync(AdminUrl));
        Assert.True(admin.RootElement.GetProperty("shareImage").GetProperty("missing").GetBoolean());

        using var client = factory.CreateClient();
        using var site = JsonDocument.Parse(await client.GetStringAsync(PublicUrl));
        Assert.Equal(JsonValueKind.Null, site.RootElement.GetProperty("shareImage").ValueKind);
    }

    private static ContentDbContext Seed(long? shareImageMediaFileId = null)
    {
        var db = new ContentDbContext(new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var settings = SiteSeoSettings.CreateEmpty(DateTimeOffset.UnixEpoch);
        if (shareImageMediaFileId is not null)
            settings.Update(new SiteSeoSettingsValues(null, null, null, null, shareImageMediaFileId),
                DateTimeOffset.UnixEpoch, "seed");
        db.SiteSeoSettings.Add(settings);
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
                .Where(id => id == 7).Distinct()
                .ToDictionary(id => id, id => new MediaImage(
                    $"https://images-kedr.cdn.express/og/{id}.jpg", $"{id}.jpg", 1200, 630)));
    }
}
