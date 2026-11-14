using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Identity.Avatars;
using IntegrationTests.TestSupport.Api;
using Media.Application.Abstractions.Persistence;
using Media.Application.Abstractions.Storage;
using Media.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Media;

/// <summary>Media library endpoints over HTTP on an in-memory store and CDN: access, uploads, folders.</summary>
public sealed class MediaApiTests
{
    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task Library_IsForManagersAndAdmins(string? role, HttpStatusCode expected)
    {
        using var factory = CreateFactory(new FakeMediaStorage());
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        Assert.Equal(expected, (await client.GetAsync("/api/admin/media/folders")).StatusCode);
        Assert.Equal(expected, (await client.GetAsync("/api/admin/media/files")).StatusCode);
    }

    [Fact]
    public async Task Manager_UploadsReplacesMovesAndRenames_ButCannotDelete()
    {
        var cdn = new FakeMediaStorage();
        using var factory = CreateFactory(cdn);
        using var manager = factory.CreateClientWithRole("Manager");
        var folder = await CreateFolder(manager, "Товари", null);

        using var upload = await manager.PostAsync("/api/admin/media/files", Form(TestImages.Create(640, 480), "731.jpg", folder));
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        using var uploaded = JsonDocument.Parse(await upload.Content.ReadAsStringAsync());
        var id = uploaded.RootElement.GetProperty("id").GetInt64();
        var url = uploaded.RootElement.GetProperty("url").GetString()!;
        Assert.Equal("731.jpg", uploaded.RootElement.GetProperty("fileName").GetString());
        Assert.Equal(640, uploaded.RootElement.GetProperty("width").GetInt32());
        Assert.StartsWith("https://images-kedr.cdn.express/library/", url);
        var firstKey = Assert.Single(cdn.Files.Keys);

        using var list = JsonDocument.Parse(await manager.GetStringAsync($"/api/admin/media/files?folderId={folder}&search=73"));
        Assert.Equal(1, list.RootElement.GetProperty("totalCount").GetInt32());

        using var replace = await manager.PutAsync($"/api/admin/media/files/{id}/content",
            Form(TestImages.Create(800, 600, SkiaSharp.SKEncodedImageFormat.Png), "new.png", null));
        Assert.Equal(HttpStatusCode.OK, replace.StatusCode);
        using var replaced = JsonDocument.Parse(await replace.Content.ReadAsStringAsync());
        Assert.Equal(id, replaced.RootElement.GetProperty("id").GetInt64());
        Assert.NotEqual(url, replaced.RootElement.GetProperty("url").GetString());
        Assert.Equal("image/png", replaced.RootElement.GetProperty("contentType").GetString());
        Assert.DoesNotContain(firstKey, cdn.Files.Keys);
        Assert.Single(cdn.Files);

        using var rename = await manager.PutAsJsonAsync($"/api/admin/media/files/{id}", new { fileName = "Петля 731.png", folderId = (long?)null });
        Assert.Equal(HttpStatusCode.OK, rename.StatusCode);
        using var move = await manager.PostAsJsonAsync("/api/admin/media/files/move", new { fileIds = new[] { id, 999 }, folderId = folder });
        using var moved = JsonDocument.Parse(await move.Content.ReadAsStringAsync());
        Assert.Equal(1, moved.RootElement.GetProperty("moved").GetInt32());

        Assert.Equal(HttpStatusCode.Forbidden, (await manager.DeleteAsync($"/api/admin/media/files/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.DeleteAsync($"/api/admin/media/folders/{folder}")).StatusCode);
    }

    [Fact]
    public async Task Upload_RejectsNonPictures_AndAnswers503WithoutCdn()
    {
        using var factory = CreateFactory(new FakeMediaStorage());
        using var manager = factory.CreateClientWithRole("Manager");
        Assert.Equal(HttpStatusCode.BadRequest,
            (await manager.PostAsync("/api/admin/media/files", Form("not a picture"u8.ToArray(), "fake.jpg", null))).StatusCode);

        using var offline = CreateFactory(new FakeMediaStorage(isConfigured: false));
        using var offlineManager = offline.CreateClientWithRole("Manager");
        Assert.Equal(HttpStatusCode.ServiceUnavailable,
            (await offlineManager.PostAsync("/api/admin/media/files", Form(TestImages.Create(10, 10), "a.jpg", null))).StatusCode);
    }

    [Fact]
    public async Task Upload_WithoutFile_AnswersValidationProblem()
    {
        using var factory = CreateFactory(new FakeMediaStorage());
        using var manager = factory.CreateClientWithRole("Manager");
        using var form = new MultipartFormDataContent { { new StringContent("1"), "folderId" } };

        using var response = await manager.PostAsync("/api/admin/media/files", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("validation", problem.RootElement.GetProperty("code").GetString());
    }

    /// <summary>Platform spec 005, SC-008: the host's cookie CSRF check still guards the moved form endpoints.</summary>
    [Fact]
    public async Task Upload_WithSessionCookieButWithoutCsrfHeader_IsRefused()
    {
        var cdn = new FakeMediaStorage();
        using var factory = CreateFactory(cdn);
        using var manager = factory.CreateClientWithRole("Manager");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/media/files")
        {
            Content = Form(TestImages.Create(10, 10), "a.jpg", null),
        };
        request.Headers.Add("Cookie", "__Host-kedr.session=stolen");

        using var response = await manager.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Сесія застаріла, оновіть сторінку.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Folders_RejectCyclesAndClashes_AndAdminDeletesOnlyEmptyOnes()
    {
        var cdn = new FakeMediaStorage();
        using var factory = CreateFactory(cdn);
        using var admin = factory.CreateClientWithRole("Admin");
        var products = await CreateFolder(admin, "Товари", null);
        var promo = await CreateFolder(admin, "Акції", products);

        Assert.Equal(HttpStatusCode.Conflict,
            (await admin.PostAsJsonAsync("/api/admin/media/folders", new { name = " акції ", parentId = products })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await admin.PutAsJsonAsync($"/api/admin/media/folders/{products}", new { name = "Товари", parentId = promo })).StatusCode);
        using var toRoot = await admin.PutAsJsonAsync($"/api/admin/media/folders/{promo}", new { name = "Знижки", parentId = (long?)null });
        Assert.Equal(HttpStatusCode.OK, toRoot.StatusCode);

        using var upload = await admin.PostAsync("/api/admin/media/files", Form(TestImages.Create(10, 10), "a.jpg", promo));
        var fileId = JsonDocument.Parse(await upload.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetInt64();
        Assert.Equal(HttpStatusCode.Conflict, (await admin.DeleteAsync($"/api/admin/media/folders/{promo}")).StatusCode);

        using var tree = JsonDocument.Parse(await admin.GetStringAsync("/api/admin/media/folders"));
        Assert.Equal(1, tree.RootElement.GetProperty("totalFileCount").GetInt32());
        Assert.Equal(2, tree.RootElement.GetProperty("folders").GetArrayLength());

        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/admin/media/files/{fileId}")).StatusCode);
        Assert.Empty(cdn.Files);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/admin/media/folders/{promo}")).StatusCode);
    }

    private static async Task<long> CreateFolder(HttpClient client, string name, long? parentId)
    {
        using var response = await client.PostAsJsonAsync("/api/admin/media/folders", new { name, parentId });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetInt64();
    }

    private static MultipartFormDataContent Form(byte[] bytes, string fileName, long? folderId)
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(file, "file", fileName);
        if (folderId is not null)
            form.Add(new StringContent(folderId.Value.ToString()), "folderId");
        return form;
    }

    private static WebApplicationFactory<Program> CreateFactory(FakeMediaStorage cdn)
    {
        var db = new MediaDbContext(new DbContextOptionsBuilder<MediaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
                services.RemoveAll<IReadMediaDbContext>();
                services.RemoveAll<MediaDbContext>();
                services.AddSingleton(db);
                services.AddSingleton<IReadMediaDbContext>(db);
                services.RemoveAll<IMediaStorage>();
                services.AddSingleton<IMediaStorage>(cdn);
            });
        });
    }
}
