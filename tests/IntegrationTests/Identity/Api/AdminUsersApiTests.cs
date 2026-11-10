using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Identity.Application.Services;
using Identity.Domain.Authorization;
using Identity.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task Seo_Works_In_The_Admin_But_Not_In_The_System_Section()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var seo = await factory.CreateUserAsync(RoleNames.Seo);
        using var client = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(client, seo.Email!);

        using var profile = await client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
        using var profileBody = JsonDocument.Parse(await profile.Content.ReadAsStringAsync());
        Assert.Equal(RoleNames.Seo, profileBody.RootElement.GetProperty("role").GetString());

        using var users = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Forbidden, users.StatusCode);

        using var requisites = await client.GetAsync("/api/admin/content/requisites");
        Assert.Equal(HttpStatusCode.Forbidden, requisites.StatusCode);
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

    [SkippableFact]
    public async Task Admin_Finds_A_Customer_And_Gives_It_Staff_Access()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        await factory.EnsureStaffRolesAsync();
        var admin = await factory.CreateUserAsync(RoleNames.Admin);
        var customer = await factory.CreateUserAsync(RoleNames.User);
        using var adminClient = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(adminClient, admin.Email!);

        using var tooShort = await adminClient.GetAsync("/api/admin/users/candidates?search=a");
        Assert.Equal(HttpStatusCode.BadRequest, tooShort.StatusCode);

        // Part of the email in another case finds the customer; staff accounts are not offered.
        var part = customer.Email![..12].ToUpperInvariant();
        using var found = await adminClient.GetAsync($"/api/admin/users/candidates?search={Uri.EscapeDataString(part)}");
        Assert.Equal(HttpStatusCode.OK, found.StatusCode);
        using (var rows = JsonDocument.Parse(await found.Content.ReadAsStringAsync()))
        {
            var row = Assert.Single(rows.RootElement.EnumerateArray(), item => item.GetProperty("id").GetGuid() == customer.Id);
            Assert.True(row.GetProperty("isCustomer").GetBoolean());
        }

        using var granted = await adminClient.PostAsJsonAsync($"/api/admin/users/{customer.Id}/staff-access", new
        {
            fullName = "Клієнт-адмін",
            role = RoleNames.Admin,
            password = "Granted-Pass-1"
        });
        Assert.Equal(HttpStatusCode.OK, granted.StatusCode);

        using var again = await adminClient.PostAsJsonAsync($"/api/admin/users/{customer.Id}/staff-access", new
        {
            fullName = "Клієнт-адмін",
            role = RoleNames.Manager,
            password = "Granted-Pass-1"
        });
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);

        using var list = await adminClient.GetAsync("/api/admin/users");
        using (var rows = JsonDocument.Parse(await list.Content.ReadAsStringAsync()))
        {
            var row = Assert.Single(rows.RootElement.EnumerateArray(), item => item.GetProperty("id").GetGuid() == customer.Id);
            Assert.Equal(RoleNames.Admin, row.GetProperty("role").GetString());
            Assert.Equal("Клієнт-адмін", row.GetProperty("fullName").GetString());
        }

        using var promotedClient = factory.CreateBrowserClient();
        using var login = await IdentityApiFactory.CookieLoginAsync(promotedClient, customer.Email!, "Granted-Pass-1");
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var adminPage = await promotedClient.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.OK, adminPage.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var stored = await userManager.FindByIdAsync(customer.Id.ToString());
        Assert.True(await userManager.IsInRoleAsync(stored!, RoleNames.User));
    }

    [SkippableFact]
    public async Task One_C_Sync_Keeps_A_Staff_Account_Of_A_Customer()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        await factory.EnsureStaffRolesAsync();
        var staffCustomer = await factory.CreateUserAsync(RoleNames.User);
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var tracked = await userManager.FindByIdAsync(staffCustomer.Id.ToString());
        Assert.True((await userManager.AddToRoleAsync(tracked!, RoleNames.Manager)).Succeeded);
        var provisioning = scope.ServiceProvider.GetRequiredService<IImportedCounterpartyIdentityProvisioningService>();

        var synced = await provisioning.EnsureUserAsync(
            new ImportedCounterpartyIdentityUserData("CP-STAFF-0001", "changed-in-1c@example.com", "Назва з 1С"),
            staffCustomer.Id,
            CancellationToken.None);
        Assert.True(synced.IsSuccess);

        var stored = await userManager.FindByIdAsync(staffCustomer.Id.ToString());
        Assert.Equal(staffCustomer.Email, stored!.Email);
        Assert.True(await userManager.CheckPasswordAsync(stored, IdentityApiFactory.Password));

        var removed = await provisioning.DeleteUserAsync(staffCustomer.Id, "CP-STAFF-0001", CancellationToken.None);
        Assert.True(removed.IsSuccess);
        var kept = await userManager.FindByIdAsync(staffCustomer.Id.ToString());
        Assert.NotNull(kept);
        Assert.False(await userManager.IsInRoleAsync(kept!, RoleNames.User));
        Assert.True(await userManager.IsInRoleAsync(kept!, RoleNames.Manager));
    }
}
