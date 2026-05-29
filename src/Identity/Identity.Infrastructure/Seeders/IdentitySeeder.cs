using Identity.Domain.Authorization;
using Identity.Infrastructure.Configuration;
using Identity.Infrastructure.Contracts;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Seeders;

public class IdentitySeeder(
    UserManager<AppUser> userManager,
    IConfiguration configuration,
    IOptions<AdminUserConfig> adminOptions,
    IOptions<TestCustomerConfig> testCustomerOptions,
    ILogger<IdentitySeeder> logger)
    : IIdentitySeeder
{
    private readonly AdminUserConfig _adminConfig = adminOptions.Value;
    private readonly TestCustomerConfig _testCustomerConfig = testCustomerOptions.Value;

    public async Task SeedAsync(IServiceProvider _, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await SeedAdminUserAsync(cancellationToken);
        await SeedTestCustomerUserAsync(cancellationToken);
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingAdmin = await userManager.FindByEmailAsync(_adminConfig.Email);
        if (existingAdmin != null)
        {
            logger.LogInformation("Admin user already exists: {Email}", _adminConfig.Email);
            return;
        }

        var password = configuration["ADMIN_DEFAULT_PASSWORD"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogError("ADMIN_DEFAULT_PASSWORD is not configured. Admin seeding is skipped.");
            return;
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = _adminConfig.Email,
            Email = _adminConfig.Email,
            FullName = _adminConfig.FullName,
            EmailConfirmed = true,
            LockoutEnabled = _adminConfig.LockoutEnabled
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to create admin user: {Errors}", errors);
            return;
        }

        result = await userManager.AddToRoleAsync(user, RoleNames.Admin);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to assign admin role: {Errors}", errors);
        }

        logger.LogInformation("Admin user created: {Email}", user.Email);
    }

    private async Task SeedTestCustomerUserAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_testCustomerConfig.IdentityUserId == Guid.Empty)
        {
            logger.LogWarning("Identity:TestCustomer:IdentityUserId is not configured. Sales test customer user seeding is skipped.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_testCustomerConfig.Email))
        {
            logger.LogWarning("Identity:TestCustomer:Email is not configured. Sales test customer user seeding is skipped.");
            return;
        }

        var user = await userManager.FindByEmailAsync(_testCustomerConfig.Email);
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(_testCustomerConfig.DefaultPassword))
            {
                logger.LogWarning(
                    "Identity:TestCustomer:DefaultPassword is not configured. Sales test customer user seeding is skipped.");
                return;
            }

            user = new AppUser
            {
                Id = _testCustomerConfig.IdentityUserId,
                UserName = _testCustomerConfig.Email,
                Email = _testCustomerConfig.Email,
                FullName = _testCustomerConfig.FullName,
                EmailConfirmed = true,
                LockoutEnabled = _testCustomerConfig.LockoutEnabled
            };

            var createResult = await userManager.CreateAsync(user, _testCustomerConfig.DefaultPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to create Sales test customer user: {Errors}", errors);
                return;
            }

            logger.LogInformation("Sales test customer user created: {Email}", user.Email);
        }

        await AddUserRoleIfMissingAsync(user, cancellationToken);
    }

    private async Task AddUserRoleIfMissingAsync(AppUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (await userManager.IsInRoleAsync(user, RoleNames.User))
        {
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            logger.LogError("Failed to assign Sales test customer role: {Errors}", errors);
        }
    }

}
