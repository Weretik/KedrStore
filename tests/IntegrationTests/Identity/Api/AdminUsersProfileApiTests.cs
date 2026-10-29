using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Identity.Domain.Authorization;
using IntegrationTests.Identity.Avatars;
using IntegrationTests.TestSupport.Database;

namespace IntegrationTests.Identity.Api;

/// <summary>An administrator edits managers on the Users page (spec identity/002, SC-008…SC-010).</summary>
[Collection(IdentityPostgresCollection.Name)]
public sealed class AdminUsersProfileApiTests(IdentityPostgresFixture fixture)
{
    [SkippableFact]
    public async Task Admin_Edits_Manager_Name_Avatar_And_List_Shows_Avatar()
    {
        fixture.SkipIfUnavailable();
        var storage = new InMemoryAvatarStorage();
        await using var factory = new IdentityApiFactory(fixture, storage);
        var admin = await factory.CreateUserAsync(RoleNames.Admin);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = await SignedInAsync(factory, admin.Email!);

        using var name = await client.PutAsJsonAsync($"/api/admin/users/{manager.Id}/name", new { fullName = "Петро" });
        Assert.Equal(HttpStatusCode.NoContent, name.StatusCode);
        using var avatar = await client.PutAsync($"/api/admin/users/{manager.Id}/avatar", AvatarRequests.Image());
        Assert.Equal(HttpStatusCode.OK, avatar.StatusCode);
        var avatarUrl = await AvatarRequests.ReadStringAsync(avatar, "avatarUrl");

        var row = await FindRowAsync(client, manager.Id);
        Assert.Equal("Петро", row.GetProperty("fullName").GetString());
        Assert.Equal(avatarUrl, row.GetProperty("avatarUrl").GetString());
        var adminRow = await FindRowAsync(client, admin.Id);
        Assert.Equal(JsonValueKind.Null, adminRow.GetProperty("avatarUrl").ValueKind);

        using var removed = await client.DeleteAsync($"/api/admin/users/{manager.Id}/avatar");
        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);
        Assert.Empty(storage.Files);
    }

    [SkippableFact]
    public async Task Admin_Changes_Manager_Email_And_Password_And_Manager_Is_Signed_Out()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var admin = await factory.CreateUserAsync(RoleNames.Admin);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        var other = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = await SignedInAsync(factory, admin.Email!);
        var refreshToken = await ProfileApiTests.MobileLoginAsync(factory, manager.Email!, IdentityApiFactory.Password);

        using var taken = await client.PutAsJsonAsync($"/api/admin/users/{manager.Id}/email", new { email = other.Email });
        Assert.Equal(HttpStatusCode.Conflict, taken.StatusCode);
        var newEmail = $"manager-new-{Guid.NewGuid():N}@example.com";
        using var email = await client.PutAsJsonAsync($"/api/admin/users/{manager.Id}/email", new { email = newEmail });
        Assert.Equal(HttpStatusCode.NoContent, email.StatusCode);

        using var refresh = await factory.CreateBrowserClient().PostAsJsonAsync("/api/auth/mobile/refresh", new { refreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
        using var login = await IdentityApiFactory.CookieLoginAsync(factory.CreateBrowserClient(), newEmail);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var password = await client.PutAsJsonAsync($"/api/admin/users/{manager.Id}/password", new { password = "Manager-New-1" });
        Assert.Equal(HttpStatusCode.NoContent, password.StatusCode);
        using var newLogin = await IdentityApiFactory.CookieLoginAsync(factory.CreateBrowserClient(), newEmail, "Manager-New-1");
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [SkippableFact]
    public async Task Admin_Cannot_Edit_Another_Admin_Or_Self_On_Users_Page()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture, new InMemoryAvatarStorage());
        var admin = await factory.CreateUserAsync(RoleNames.Admin);
        var otherAdmin = await factory.CreateUserAsync(RoleNames.Admin);
        using var client = await SignedInAsync(factory, admin.Email!);

        foreach (var target in new[] { otherAdmin.Id, admin.Id })
        {
            using var name = await client.PutAsJsonAsync($"/api/admin/users/{target}/name", new { fullName = "X" });
            using var email = await client.PutAsJsonAsync($"/api/admin/users/{target}/email", new { email = "x@example.com" });
            using var password = await client.PutAsJsonAsync($"/api/admin/users/{target}/password", new { password = "Another-Pass-1" });
            using var avatar = await client.PutAsync($"/api/admin/users/{target}/avatar", AvatarRequests.Image());
            using var removeAvatar = await client.DeleteAsync($"/api/admin/users/{target}/avatar");

            Assert.All(
                new[] { name, email, password, avatar, removeAvatar },
                response => Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode));
        }

        using var missing = await client.PutAsJsonAsync($"/api/admin/users/{Guid.NewGuid()}/name", new { fullName = "X" });
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [SkippableFact]
    public async Task Manager_Cannot_Use_Users_Page_Edits()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        var other = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = await SignedInAsync(factory, manager.Email!);

        using var name = await client.PutAsJsonAsync($"/api/admin/users/{other.Id}/name", new { fullName = "X" });
        Assert.Equal(HttpStatusCode.Forbidden, name.StatusCode);
    }

    private static async Task<HttpClient> SignedInAsync(IdentityApiFactory factory, string email)
    {
        var client = factory.CreateBrowserClient();
        using var login = await IdentityApiFactory.CookieLoginAsync(client, email);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        return client;
    }

    private static async Task<JsonElement> FindRowAsync(HttpClient client, Guid id)
    {
        using var list = await client.GetAsync("/api/admin/users");
        using var json = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        return json.RootElement.EnumerateArray().Single(row => row.GetProperty("id").GetGuid() == id).Clone();
    }
}
