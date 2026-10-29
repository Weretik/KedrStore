using System.Net;
using System.Net.Http.Json;
using Identity.Domain.Authorization;
using IntegrationTests.Identity.Avatars;
using IntegrationTests.TestSupport.Database;

namespace IntegrationTests.Identity.Api;

/// <summary>Own profile through the real API (spec identity/002, SC-001…SC-007, SC-011).</summary>
[Collection(IdentityPostgresCollection.Name)]
public sealed class ProfileApiTests(IdentityPostgresFixture fixture)
{
    [SkippableFact]
    public async Task Manager_Reads_Profile_Customer_Gets_403_Anonymous_Gets_401()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        var customer = await factory.CreateUserAsync(RoleNames.User);

        using var client = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(client, manager.Email!);
        using var profile = await client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
        var body = await profile.Content.ReadFromJsonAsync<ProfileBody>();
        Assert.Equal(manager.Email, body!.Email);
        Assert.Equal(RoleNames.Manager, body.Role);
        Assert.Null(body.AvatarUrl);

        using var customerClient = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(customerClient, customer.Email!);
        using var forbidden = await customerClient.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        using var anonymous = await factory.CreateBrowserClient().GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
    }

    [SkippableFact]
    public async Task Manager_Changes_Own_Name_And_Empty_Name_Is_Rejected()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(client, manager.Email!);

        using var changed = await client.PutAsJsonAsync("/api/profile/name", new { fullName = "  Олена  " });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        using var empty = await client.PutAsJsonAsync("/api/profile/name", new { fullName = "  " });
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);

        using var me = await client.GetAsync("/api/auth/session/me");
        Assert.Equal("Олена", await AvatarRequests.ReadStringAsync(me, "fullName"));
    }

    [SkippableFact]
    public async Task Avatar_Upload_Replace_And_Remove_Keep_Only_The_Current_File()
    {
        fixture.SkipIfUnavailable();
        var storage = new InMemoryAvatarStorage();
        await using var factory = new IdentityApiFactory(fixture, storage);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(client, manager.Email!);

        using var first = await client.PutAsync("/api/profile/avatar", AvatarRequests.Image(1000, 600));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstUrl = await AvatarRequests.ReadStringAsync(first, "avatarUrl");
        Assert.StartsWith($"https://cdn.test/avatars/{manager.Id:N}/", firstUrl);
        Assert.Single(storage.Files);

        using var second = await client.PutAsync("/api/profile/avatar", AvatarRequests.Image());
        var secondUrl = await AvatarRequests.ReadStringAsync(second, "avatarUrl");
        Assert.NotEqual(firstUrl, secondUrl);
        var key = Assert.Single(storage.Files).Key;
        Assert.EndsWith(key, secondUrl);

        using var me = await client.GetAsync("/api/auth/session/me");
        Assert.Equal(secondUrl, await AvatarRequests.ReadStringAsync(me, "avatarUrl"));

        using var removed = await client.DeleteAsync("/api/profile/avatar");
        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);
        Assert.Empty(storage.Files);
        using var profile = await client.GetAsync("/api/profile");
        Assert.Null(await AvatarRequests.ReadStringAsync(profile, "avatarUrl"));

        using var removedAgain = await client.DeleteAsync("/api/profile/avatar");
        Assert.Equal(HttpStatusCode.NoContent, removedAgain.StatusCode);
    }

    [SkippableFact]
    public async Task Small_Or_Non_Image_Upload_Is_Rejected_And_Nothing_Is_Stored()
    {
        fixture.SkipIfUnavailable();
        var storage = new InMemoryAvatarStorage();
        await using var factory = new IdentityApiFactory(fixture, storage);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(client, manager.Email!);

        using var small = await client.PutAsync("/api/profile/avatar", AvatarRequests.Image(100, 100));
        Assert.Equal(HttpStatusCode.BadRequest, small.StatusCode);
        using var text = await client.PutAsync("/api/profile/avatar", AvatarRequests.File("hello"u8.ToArray(), "text/plain"));
        Assert.Equal(HttpStatusCode.BadRequest, text.StatusCode);
        Assert.Empty(storage.Files);
    }

    [SkippableFact]
    public async Task Upload_Without_Storage_Returns_503_But_Name_Still_Changes()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(client, manager.Email!);

        using var upload = await client.PutAsync("/api/profile/avatar", AvatarRequests.Image());
        Assert.Equal(HttpStatusCode.ServiceUnavailable, upload.StatusCode);
        using var name = await client.PutAsJsonAsync("/api/profile/name", new { fullName = "Ім'я" });
        Assert.Equal(HttpStatusCode.NoContent, name.StatusCode);
    }

    [SkippableFact]
    public async Task Manager_Cannot_Change_Own_Email_Or_Password()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(client, manager.Email!);

        using var email = await client.PutAsJsonAsync("/api/profile/email", new { email = "x@example.com", currentPassword = IdentityApiFactory.Password });
        Assert.Equal(HttpStatusCode.Forbidden, email.StatusCode);
        using var password = await client.PutAsJsonAsync("/api/profile/password", new { currentPassword = IdentityApiFactory.Password, newPassword = "Another-Pass-1" });
        Assert.Equal(HttpStatusCode.Forbidden, password.StatusCode);
    }

    [SkippableFact]
    public async Task Admin_Changes_Own_Email_Keeps_Current_Session_And_Ends_Mobile_Sessions()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var admin = await factory.CreateUserAsync(RoleNames.Admin);
        var other = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(client, admin.Email!);
        var refreshToken = await MobileLoginAsync(factory, admin.Email!, IdentityApiFactory.Password);

        using var wrong = await client.PutAsJsonAsync("/api/profile/email", new { email = "new@example.com", currentPassword = "Wrong-Pass-1" });
        Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);
        using var taken = await client.PutAsJsonAsync("/api/profile/email", new { email = other.Email, currentPassword = IdentityApiFactory.Password });
        Assert.Equal(HttpStatusCode.Conflict, taken.StatusCode);

        var newEmail = $"admin-new-{Guid.NewGuid():N}@example.com";
        using var changed = await client.PutAsJsonAsync("/api/profile/email", new { email = newEmail, currentPassword = IdentityApiFactory.Password });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);

        using var me = await client.GetAsync("/api/auth/session/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        Assert.Equal(newEmail, await AvatarRequests.ReadStringAsync(me, "email"));

        using var refresh = await factory.CreateBrowserClient().PostAsJsonAsync("/api/auth/mobile/refresh", new { refreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);

        using var login = await IdentityApiFactory.CookieLoginAsync(factory.CreateBrowserClient(), newEmail);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [SkippableFact]
    public async Task Admin_Changes_Own_Password_With_Current_Password()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var admin = await factory.CreateUserAsync(RoleNames.Admin);
        using var client = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(client, admin.Email!);

        using var wrong = await client.PutAsJsonAsync("/api/profile/password", new { currentPassword = "Wrong-Pass-1", newPassword = "Another-Pass-1" });
        Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);
        using var shortPassword = await client.PutAsJsonAsync("/api/profile/password", new { currentPassword = IdentityApiFactory.Password, newPassword = "short" });
        Assert.Equal(HttpStatusCode.BadRequest, shortPassword.StatusCode);

        using var changed = await client.PutAsJsonAsync("/api/profile/password", new { currentPassword = IdentityApiFactory.Password, newPassword = "Another-Pass-1" });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        using var me = await client.GetAsync("/api/auth/session/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);

        using var oldLogin = await IdentityApiFactory.CookieLoginAsync(factory.CreateBrowserClient(), admin.Email!);
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
        using var newLogin = await IdentityApiFactory.CookieLoginAsync(factory.CreateBrowserClient(), admin.Email!, "Another-Pass-1");
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    internal static async Task<string> MobileLoginAsync(IdentityApiFactory factory, string email, string password)
    {
        using var response = await factory.CreateBrowserClient().PostAsJsonAsync("/api/auth/mobile/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await AvatarRequests.ReadStringAsync(response, "refreshToken"))!;
    }

    private sealed record ProfileBody(Guid Id, string Email, string FullName, string Role, string? AvatarUrl);
}
