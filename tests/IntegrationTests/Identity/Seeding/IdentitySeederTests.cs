using Identity.Domain.Authorization;
using Identity.Infrastructure.Contracts;
using Identity.Infrastructure.DependencyInjection;
using Identity.Infrastructure.Entities;
using IntegrationTests.TestSupport.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Identity.Seeding;

[Collection(IdentityPostgresCollection.Name)]
public sealed class IdentitySeederTests(IdentityPostgresFixture fixture)
{
    [SkippableFact]
    public async Task Seeders_Create_Configured_Administrator_With_Lockout()
    {
        fixture.SkipIfUnavailable();
        var email = $"seed-admin-{Guid.NewGuid():N}@example.com";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = fixture.ConnectionString,
                ["Identity:AdminUser:Email"] = email,
                ["Identity:AdminUser:FullName"] = "Seeded admin",
                ["ADMIN_DEFAULT_PASSWORD"] = "Seed-Password-1"
            })
            .Build();

        // The same registration the migration job uses; it must bind the administrator settings itself.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddIdentityInfrastructureServices(configuration);
        await using var provider = services.BuildServiceProvider();

        await using (var scope = provider.CreateAsyncScope())
        {
            foreach (var seeder in scope.ServiceProvider.GetServices<IIdentitySeeder>())
            {
                await seeder.SeedAsync(scope.ServiceProvider);
            }
        }

        await using var check = provider.CreateAsyncScope();
        var userManager = check.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var admin = await userManager.FindByEmailAsync(email);
        Assert.NotNull(admin);
        Assert.True(admin.LockoutEnabled);
        Assert.True(await userManager.IsInRoleAsync(admin, RoleNames.Admin));
        Assert.True(await userManager.CheckPasswordAsync(admin, "Seed-Password-1"));
    }
}
