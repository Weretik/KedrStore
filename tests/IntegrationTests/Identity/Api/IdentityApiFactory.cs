using System.Net.Http.Json;
using Identity.Domain.Authorization;
using Identity.Infrastructure.Entities;
using IntegrationTests.TestSupport.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Identity.Api;

/// <summary>
/// The real API over the Identity test database. Clients keep cookies like a browser.
/// </summary>
internal sealed class IdentityApiFactory(IdentityPostgresFixture fixture) : WebApplicationFactory<Program>
{
    public const string Password = "Staff-Password-1";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", fixture.ConnectionString);
    }

    public HttpClient CreateBrowserClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), HandleCookies = true });

    public async Task<AppUser> CreateUserAsync(string role)
    {
        await using var scope = Services.CreateAsyncScope();
        await EnsureRoleAsync(scope.ServiceProvider, role);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var email = $"{role.ToLowerInvariant()}-{Guid.NewGuid():N}@example.com";
        var user = new AppUser { Id = Guid.NewGuid(), UserName = email, Email = email, FullName = role, EmailConfirmed = true, LockoutEnabled = true };
        var created = await userManager.CreateAsync(user, Password);
        Assert.True(created.Succeeded, string.Join(", ", created.Errors.Select(e => e.Description)));
        Assert.True((await userManager.AddToRoleAsync(user, role)).Succeeded);
        return user;
    }

    public async Task EnsureStaffRolesAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        await EnsureRoleAsync(scope.ServiceProvider, RoleNames.Admin);
        await EnsureRoleAsync(scope.ServiceProvider, RoleNames.Manager);
    }

    private static async Task EnsureRoleAsync(IServiceProvider services, string role)
    {
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new AppRole { Id = Guid.NewGuid(), Name = role, NormalizedName = role.ToUpperInvariant() });
        }
    }

    /// <summary>Signs a browser client in with the session cookie and sets its CSRF header.</summary>
    public static async Task<HttpResponseMessage> CookieLoginAsync(HttpClient client, string email, string password = Password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/cookie/login", new { email, password });
        var csrf = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.Select(c => c.Split(';')[0]).FirstOrDefault(c => c.StartsWith("kedr.csrf=", StringComparison.Ordinal))
            : null;
        client.DefaultRequestHeaders.Remove("X-CSRF-Token");
        if (csrf is not null)
        {
            client.DefaultRequestHeaders.Add("X-CSRF-Token", csrf["kedr.csrf=".Length..]);
        }

        return response;
    }
}
