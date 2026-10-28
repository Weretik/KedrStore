using IntegrationTests.TestSupport.Database;

namespace IntegrationTests.Identity.Sessions;

[Collection(IdentityPostgresCollection.Name)]
public sealed class IdentitySessionHardeningTests(IdentityPostgresFixture fixture)
{
    [SkippableFact]
    public async Task Login_Persists_Data_Protection_Keys_In_Database()
    {
        fixture.SkipIfUnavailable();
        await using var harness = IdentitySessionHarness.Create(fixture);
        var user = await harness.CreateUserAsync();

        var login = await harness.RequestAsync(s => s.LoginAsync(user.Email!, IdentitySessionHarness.Password));

        Assert.NotNull(login);
        Assert.True(await harness.DataProtectionKeyCountAsync() > 0);
    }

    [SkippableFact]
    public async Task Five_Wrong_Passwords_Lock_The_Account()
    {
        fixture.SkipIfUnavailable();
        await using var harness = IdentitySessionHarness.Create(fixture);
        var user = await harness.CreateUserAsync();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            Assert.Null(await harness.RequestAsync(s => s.LoginAsync(user.Email!, "wrong-password")));
        }

        var stored = await harness.ReloadUserAsync(user.Id);
        Assert.True(stored.LockoutEnabled);
        Assert.True(stored.LockoutEnd > DateTimeOffset.UtcNow.AddMinutes(14));
        Assert.Null(await harness.RequestAsync(s => s.LoginAsync(user.Email!, IdentitySessionHarness.Password)));
    }

    [SkippableFact]
    public async Task Rotated_Token_Within_Grace_Fails_Without_Ending_Other_Sessions()
    {
        fixture.SkipIfUnavailable();
        await using var harness = IdentitySessionHarness.Create(fixture);
        var user = await harness.CreateUserAsync();
        var login = await harness.RequestAsync(s => s.LoginAsync(user.Email!, IdentitySessionHarness.Password));

        var first = await harness.RequestAsync(s => s.RefreshAsync(login!.RefreshToken));
        var repeated = await harness.RequestAsync(s => s.RefreshAsync(login!.RefreshToken));

        Assert.NotNull(first);
        Assert.Null(repeated);
        var sessions = await harness.SessionsOfAsync(user.Id);
        Assert.Contains(sessions, x => x.RevokedAtUtc is null);
        Assert.DoesNotContain(sessions, x => x.ReuseDetectedAtUtc is not null);
    }

    [SkippableFact]
    public async Task Rotated_Token_After_Grace_Is_Reuse_And_Ends_All_Sessions()
    {
        fixture.SkipIfUnavailable();
        await using var harness = IdentitySessionHarness.Create(fixture, graceSeconds: 0);
        var user = await harness.CreateUserAsync();
        var login = await harness.RequestAsync(s => s.LoginAsync(user.Email!, IdentitySessionHarness.Password));

        Assert.NotNull(await harness.RequestAsync(s => s.RefreshAsync(login!.RefreshToken)));
        Assert.Null(await harness.RequestAsync(s => s.RefreshAsync(login!.RefreshToken)));

        var sessions = await harness.SessionsOfAsync(user.Id);
        Assert.All(sessions, x => Assert.NotNull(x.RevokedAtUtc));
        Assert.Contains(sessions, x => x.ReuseDetectedAtUtc is not null);
    }

    [SkippableFact]
    public async Task Parallel_Refresh_Rotates_Once_And_Keeps_Session_Alive()
    {
        fixture.SkipIfUnavailable();
        await using var harness = IdentitySessionHarness.Create(fixture);
        var user = await harness.CreateUserAsync();
        var login = await harness.RequestAsync(s => s.LoginAsync(user.Email!, IdentitySessionHarness.Password));

        var results = await Task.WhenAll(Enumerable.Range(0, 4)
            .Select(_ => Task.Run(() => harness.RequestAsync(s => s.RefreshAsync(login!.RefreshToken)))));

        Assert.Single(results, x => x is not null);
        var sessions = await harness.SessionsOfAsync(user.Id);
        Assert.Equal(2, sessions.Count);
        Assert.Single(sessions, x => x.RevokedAtUtc is null);
        Assert.DoesNotContain(sessions, x => x.ReuseDetectedAtUtc is not null);
    }

    [SkippableFact]
    public async Task Logout_With_Refresh_Token_Ends_Only_That_Session()
    {
        fixture.SkipIfUnavailable();
        await using var harness = IdentitySessionHarness.Create(fixture);
        var user = await harness.CreateUserAsync();
        var laptop = await harness.RequestAsync(s => s.LoginAsync(user.Email!, IdentitySessionHarness.Password));
        var phone = await harness.RequestAsync(s => s.LoginAsync(user.Email!, IdentitySessionHarness.Password));

        Assert.True(await harness.RequestAsync(s => s.LogoutAsync(laptop!.RefreshToken), user));

        Assert.Null(await harness.RequestAsync(s => s.RefreshAsync(laptop!.RefreshToken)));
        Assert.NotNull(await harness.RequestAsync(s => s.RefreshAsync(phone!.RefreshToken)));
    }

    [SkippableFact]
    public async Task Logout_Without_Refresh_Token_Ends_All_Sessions()
    {
        fixture.SkipIfUnavailable();
        await using var harness = IdentitySessionHarness.Create(fixture);
        var user = await harness.CreateUserAsync();
        var laptop = await harness.RequestAsync(s => s.LoginAsync(user.Email!, IdentitySessionHarness.Password));
        var phone = await harness.RequestAsync(s => s.LoginAsync(user.Email!, IdentitySessionHarness.Password));

        Assert.True(await harness.RequestAsync(s => s.LogoutAsync(refreshToken: null), user));

        var sessions = await harness.SessionsOfAsync(user.Id);
        Assert.All(sessions, x => Assert.Equal("Logout", x.RevocationReason));
    }
}
