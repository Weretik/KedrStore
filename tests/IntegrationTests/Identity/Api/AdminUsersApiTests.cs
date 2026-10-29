using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Identity.Domain.Authorization;
using IntegrationTests.TestSupport.Database;

namespace IntegrationTests.Identity.Api;

[Collection(IdentityPostgresCollection.Name)]
public sealed class AdminUsersApiTests(IdentityPostgresFixture fixture)
{
    [SkippableFact]
    public async Task Anonymous_Gets_401_And_Manager_Gets_403()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);

        using var anonymous = factory.CreateBrowserClient();
        using var anonymousResponse = await anonymous.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        using var managerClient = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(managerClient, manager.Email!);
        using var managerResponse = await managerClient.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Forbidden, managerResponse.StatusCode);
    }

    [SkippableFact]
    public async Task Admin_Creates_Manager_Who_Can_Sign_In()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        await factory.EnsureStaffRolesAsync();
        var admin = await factory.CreateUserAsync(RoleNames.Admin);
        using var adminClient = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(adminClient, admin.Email!);
        var email = $"new-manager-{Guid.NewGuid():N}@example.com";

        using var created = await adminClient.PostAsJsonAsync("/api/admin/users", new
        {
            email,
            fullName = "Новий менеджер",
            role = RoleNames.Manager,
            password = "Manager-Pass-1"
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        using var duplicate = await adminClient.PostAsJsonAsync("/api/admin/users", new
        {
            email,
            fullName = "Дубль",
            role = RoleNames.Manager,
            password = "Manager-Pass-1"
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        using var list = await adminClient.GetAsync("/api/admin/users");
        using var rows = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Contains(rows.RootElement.EnumerateArray(), row => row.GetProperty("email").GetString() == email);

        using var managerClient = factory.CreateBrowserClient();
        using var login = await IdentityApiFactory.CookieLoginAsync(managerClient, email, "Manager-Pass-1");
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [SkippableFact]
    public async Task Short_Password_And_Unknown_Role_Are_Rejected()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var admin = await factory.CreateUserAsync(RoleNames.Admin);
        using var adminClient = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(adminClient, admin.Email!);

        using var response = await adminClient.PostAsJsonAsync("/api/admin/users", new
        {
            email = "someone@example.com",
            fullName = "Someone",
            role = RoleNames.User,
            password = "short"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [SkippableFact]
    public async Task Blocked_Manager_Cannot_Sign_In_And_Admin_Cannot_Block_Self()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var admin = await factory.CreateUserAsync(RoleNames.Admin);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var adminClient = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(adminClient, admin.Email!);

        using var self = await adminClient.PutAsJsonAsync($"/api/admin/users/{admin.Id}/blocked", new { blocked = true });
        Assert.Equal(HttpStatusCode.BadRequest, self.StatusCode);

        using var block = await adminClient.PutAsJsonAsync($"/api/admin/users/{manager.Id}/blocked", new { blocked = true });
        Assert.Equal(HttpStatusCode.NoContent, block.StatusCode);

        using var managerClient = factory.CreateBrowserClient();
        using var blockedLogin = await IdentityApiFactory.CookieLoginAsync(managerClient, manager.Email!);
        Assert.Equal(HttpStatusCode.Unauthorized, blockedLogin.StatusCode);

        using var unblock = await adminClient.PutAsJsonAsync($"/api/admin/users/{manager.Id}/blocked", new { blocked = false });
        Assert.Equal(HttpStatusCode.NoContent, unblock.StatusCode);
        using var login = await IdentityApiFactory.CookieLoginAsync(managerClient, manager.Email!);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [SkippableFact]
    public async Task Admin_Changes_Role_And_Sets_Password()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var admin = await factory.CreateUserAsync(RoleNames.Admin);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var adminClient = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(adminClient, admin.Email!);

        // A password is set for managers only, so it comes before the promotion.
        using var password = await adminClient.PutAsJsonAsync($"/api/admin/users/{manager.Id}/password", new { password = "Changed-Pass-1" });
        Assert.Equal(HttpStatusCode.NoContent, password.StatusCode);
        using var role = await adminClient.PutAsJsonAsync($"/api/admin/users/{manager.Id}/role", new { role = RoleNames.Admin });
        Assert.Equal(HttpStatusCode.NoContent, role.StatusCode);

        using var promotedClient = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(promotedClient, manager.Email!, "Changed-Pass-1");
        using var list = await promotedClient.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
    }
}
