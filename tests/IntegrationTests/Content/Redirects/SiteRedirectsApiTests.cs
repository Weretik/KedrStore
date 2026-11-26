using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestSupport.Api;
using Microsoft.EntityFrameworkCore;
using static IntegrationTests.Content.Redirects.SiteRedirectsTestHost;

namespace IntegrationTests.Content.Redirects;

/// <summary>Spec content/013: site redirects over HTTP.</summary>
public sealed class SiteRedirectsApiTests
{
    private const string Admin = "/api/admin/content/redirects";

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task AdminList_IsForContentEditors(string? role, HttpStatusCode expected)
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.GetAsync(Admin);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Create_StoresTheLookupPath_AndTheSiteReadsItAnonymously()
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var created = await manager.PostAsJsonAsync(Admin,
            new { from = "https://classkedr.com.ua/UK/Stara-Storinka/?utm=1", to = "/uk/catalog/ruchky" });
        using var site = JsonDocument.Parse(await factory.CreateClient().GetStringAsync("/api/content/redirects"));
        using var list = JsonDocument.Parse(await manager.GetStringAsync(Admin + "?page=1&pageSize=10"));

        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var entry = Assert.Single(site.RootElement.EnumerateArray());
        Assert.Equal("/uk/stara-storinka", entry.GetProperty("from").GetString());
        Assert.Equal("/uk/catalog/ruchky", entry.GetProperty("to").GetString());
        Assert.Equal(1, list.RootElement.GetProperty("totalCount").GetInt32());
        Assert.False(string.IsNullOrEmpty(list.RootElement.GetProperty("items")[0].GetProperty("updatedBy").GetString()));
    }

    [Fact]
    public async Task Create_RefusesADuplicateAChainAndASelfRedirect()
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");
        using var first = await manager.PostAsJsonAsync(Admin, new { from = "/uk/a", to = "/uk/b" });

        using var duplicate = await manager.PostAsJsonAsync(Admin, new { from = "/UK/A/", to = "/uk/c" });
        using var toRedirected = await manager.PostAsJsonAsync(Admin, new { from = "/uk/x", to = "/uk/a" });
        using var intoOld = await manager.PostAsJsonAsync(Admin, new { from = "/uk/b", to = "/uk/c" });
        using var self = await manager.PostAsJsonAsync(Admin, new { from = "/uk/d", to = "/uk/d?x=1" });
        using var reserved = await manager.PostAsJsonAsync(Admin, new { from = "/api/x", to = "/uk" });

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, toRedirected.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, intoOld.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, self.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, reserved.StatusCode);
        Assert.Contains("\"from\"", await reserved.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, await db.SiteRedirects.CountAsync());
    }

    [Fact]
    public async Task Update_ReplacesBothAddresses_AndRefusesAStaleVersion()
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");
        using var created = await manager.PostAsJsonAsync(Admin, new { from = "/uk/a", to = "/uk/b" });
        using var body = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = body.RootElement.GetProperty("id").GetInt64();
        var version = body.RootElement.GetProperty("version").GetUInt32();

        using var updated = await manager.PutAsJsonAsync($"{Admin}/{id}", new { from = "/uk/a", to = "/uk/c", version });
        using var stale = await manager.PutAsJsonAsync($"{Admin}/{id}", new { from = "/uk/a", to = "/uk/d", version = version + 7 });
        using var missing = await manager.PutAsJsonAsync($"{Admin}/999", new { from = "/uk/a", to = "/uk/d" });

        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("/uk/c", (await db.SiteRedirects.AsNoTracking().SingleAsync()).ToUrl);
    }

    [Fact]
    public async Task Delete_RemovesTheRedirect_AndARepeatAnswers404()
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");
        using var created = await manager.PostAsJsonAsync(Admin, new { from = "/uk/a", to = "/uk/b" });
        var id = JsonDocument.Parse(await created.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetInt64();

        using var deleted = await manager.DeleteAsync($"{Admin}/{id}");
        using var repeat = await manager.DeleteAsync($"{Admin}/{id}");

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, repeat.StatusCode);
        Assert.Equal(0, await db.SiteRedirects.CountAsync());
    }
}
