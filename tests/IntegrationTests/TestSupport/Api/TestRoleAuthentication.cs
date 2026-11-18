using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IntegrationTests.TestSupport.Api;

/// <summary>
/// Replaces real sign-in in API tests: a request with <see cref="RoleHeader"/> is a user in that role;
/// a request without it is anonymous. <see cref="UserHeader"/> optionally sets the user ID (default <c>test-user</c>).
/// </summary>
public static class TestRoleAuthentication
{
    public const string SchemeName = "TestRole";
    public const string RoleHeader = "X-Test-Role";
    public const string UserHeader = "X-Test-User";
    /// <summary>The account id of a client without <see cref="UserHeader"/>; real account ids are Guids.</summary>
    public const string DefaultUserId = "0b1e4c2a-5d6f-4e7a-8b9c-0d1e2f3a4b5c";

    public static IServiceCollection AddTestRoleAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = SchemeName;
                options.DefaultChallengeScheme = SchemeName;
                options.DefaultForbidScheme = SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, Handler>(SchemeName, _ => { });
        return services;
    }

    public static HttpClient CreateClientWithRole(this WebApplicationFactory<Program> factory, string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(RoleHeader, role);
        return client;
    }

    public static HttpClient CreateClientWithUser(this WebApplicationFactory<Program> factory, string role, Guid userId)
    {
        var client = factory.CreateClientWithRole(role);
        client.DefaultRequestHeaders.Add(UserHeader, userId.ToString());
        return client;
    }

    private sealed class Handler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(RoleHeader, out var role) || string.IsNullOrWhiteSpace(role))
                return Task.FromResult(AuthenticateResult.NoResult());

            var userId = Request.Headers.TryGetValue(UserHeader, out var user) && !string.IsNullOrWhiteSpace(user)
                ? user.ToString()
                : DefaultUserId;
            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, role.ToString())],
                SchemeName);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
        }
    }
}
