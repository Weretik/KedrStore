using System.Security.Claims;
using Identity.Application.Services;
using Identity.Infrastructure.DataBase;
using Identity.Infrastructure.DependencyInjection;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Options;
using IntegrationTests.TestSupport.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Identity.Sessions;

/// <summary>
/// Real Identity session services over the test PostgreSQL database, one HTTP request per scope.
/// </summary>
internal sealed class IdentitySessionHarness : IAsyncDisposable
{
    public const string Password = "Correct-Password-1";

    private readonly ServiceProvider _provider;

    private IdentitySessionHarness(ServiceProvider provider) => _provider = provider;

    public static IdentitySessionHarness Create(IdentityPostgresFixture fixture, int graceSeconds = 60)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = fixture.ConnectionString,
                [$"{IdentitySessionSecurityOptions.SectionName}:RefreshReuseGraceSeconds"] = graceSeconds.ToString()
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddIdentityInfrastructureServices(configuration);
        services.Configure<IdentitySessionSecurityOptions>(configuration.GetSection(IdentitySessionSecurityOptions.SectionName));
        services.Configure<IdentitySessionPerformanceOptions>(_ => { });
        return new IdentitySessionHarness(services.BuildServiceProvider());
    }

    public async Task<AppUser> CreateUserAsync()
    {
        await using var scope = _provider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var user = new AppUser { Id = Guid.NewGuid(), UserName = email, Email = email, FullName = "Test", EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, Password);
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
        return user;
    }

    /// <summary>Runs one request; <paramref name="user"/> becomes the authenticated principal when given.</summary>
    public async Task<T> RequestAsync<T>(Func<IIdentitySessionService, Task<T>> action, AppUser? user = null)
    {
        await using var scope = _provider.CreateAsyncScope();
        var httpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        if (user is not null)
        {
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())],
                "Test"));
        }

        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = httpContext;
        return await action(scope.ServiceProvider.GetRequiredService<IIdentitySessionService>());
    }

    public async Task<List<AppRefreshSession>> SessionsOfAsync(Guid userId)
    {
        await using var scope = _provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        return await db.RefreshSessions.AsNoTracking().Where(x => x.UserId == userId).OrderBy(x => x.Id).ToListAsync();
    }

    public async Task<AppUser> ReloadUserAsync(Guid userId)
    {
        await using var scope = _provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        return await db.Users.AsNoTracking().SingleAsync(x => x.Id == userId);
    }

    public async Task<int> DataProtectionKeyCountAsync()
    {
        await using var scope = _provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        return await db.DataProtectionKeys.CountAsync();
    }

    public ValueTask DisposeAsync() => _provider.DisposeAsync();
}
