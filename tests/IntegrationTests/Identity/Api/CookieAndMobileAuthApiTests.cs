using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Identity.Domain.Authorization;
using IntegrationTests.TestSupport.Database;

namespace IntegrationTests.Identity.Api;

[Collection(IdentityPostgresCollection.Name)]
public sealed class CookieAndMobileAuthApiTests(IdentityPostgresFixture fixture)
{
    [SkippableFact]
    public async Task Cookie_Login_Sets_HttpOnly_Session_And_Me_Works()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = factory.CreateBrowserClient();

        using var login = await IdentityApiFactory.CookieLoginAsync(client, manager.Email!);

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var sessionCookie = Assert.Single(login.Headers.GetValues("Set-Cookie"), c => c.StartsWith("__Host-kedr.session=", StringComparison.Ordinal));
        Assert.Contains("httponly", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("accessToken", await login.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        using var me = await client.GetAsync("/api/auth/session/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        using var body = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        Assert.Equal(manager.Email, body.RootElement.GetProperty("email").GetString());
        Assert.Equal(RoleNames.Manager, body.RootElement.GetProperty("roles")[0].GetString());
    }

    [SkippableFact]
    public async Task Cookie_Session_Post_Requires_Csrf_Header()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = factory.CreateBrowserClient();
        await IdentityApiFactory.CookieLoginAsync(client, manager.Email!);
        var csrf = client.DefaultRequestHeaders.GetValues("X-CSRF-Token").Single();

        client.DefaultRequestHeaders.Remove("X-CSRF-Token");
        using var withoutHeader = await client.PostAsync("/api/auth/cookie/logout", null);
        Assert.Equal(HttpStatusCode.BadRequest, withoutHeader.StatusCode);

        client.DefaultRequestHeaders.Add("X-CSRF-Token", csrf);
        using var withHeader = await client.PostAsync("/api/auth/cookie/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, withHeader.StatusCode);

        using var me = await client.GetAsync("/api/auth/session/me");
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [SkippableFact]
    public async Task Wrong_Cookie_Password_Returns_401()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = factory.CreateBrowserClient();

        using var login = await IdentityApiFactory.CookieLoginAsync(client, manager.Email!, "wrong-password");

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [SkippableFact]
    public async Task Mobile_Login_Refresh_And_Logout_Use_Body_Tokens()
    {
        fixture.SkipIfUnavailable();
        await using var factory = new IdentityApiFactory(fixture);
        var manager = await factory.CreateUserAsync(RoleNames.Manager);
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), HandleCookies = false });

        using var login = await client.PostAsJsonAsync("/api/auth/mobile/login", new { email = manager.Email, password = IdentityApiFactory.Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.False(login.Headers.Contains("Set-Cookie"));
        var tokens = await login.Content.ReadFromJsonAsync<TokenPair>();

        using var refresh = await client.PostAsJsonAsync("/api/auth/mobile/refresh", new { refreshToken = tokens!.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        var rotated = await refresh.Content.ReadFromJsonAsync<TokenPair>();
        Assert.NotEqual(tokens.RefreshToken, rotated!.RefreshToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rotated.AccessToken);
        using var me = await client.GetAsync("/api/auth/session/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);

        // A bearer request is not a cookie session, so it needs no CSRF header.
        using var logout = await client.PostAsJsonAsync("/api/auth/mobile/logout", new { refreshToken = rotated.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        using var afterLogout = await client.PostAsJsonAsync("/api/auth/mobile/refresh", new { refreshToken = rotated.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
    }

    private sealed record TokenPair(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);
}
