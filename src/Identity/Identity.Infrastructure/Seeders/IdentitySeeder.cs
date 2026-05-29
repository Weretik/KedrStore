using Identity.Domain.Authorization;
using Identity.Infrastructure.Contracts;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Options;

namespace Identity.Infrastructure.Seeders;

public class IdentitySeeder(
    UserManager<AppUser> userManager,
    IConfiguration configuration,
    IOptions<AdminUserOptions> adminOptions,
    IOptions<TestCustomerOptions> testCustomerOptions,
    ILogger<IdentitySeeder> logger)
    : IIdentitySeeder
{
    private readonly AdminUserOptions _adminOptions = adminOptions.Value;
    private readonly TestCustomerOptions _testCustomerOptions = testCustomerOptions.Value;

    public async Task SeedAsync(IServiceProvider _, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await SeedAdminUserAsync(cancellationToken);
        await SeedTestCustomerUserAsync(cancellationToken);
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingAdmin = await userManager.FindByEmailAsync(_adminOptions.Email);
        if (existingAdmin != null)
        {
            logger.LogInformation("Admin user already exists: {Email}", _adminOptions.Email);
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
            UserName = _adminOptions.Email,
            Email = _adminOptions.Email,
            FullName = _adminOptions.FullName,
            EmailConfirmed = true,
            LockoutEnabled = _adminOptions.LockoutEnabled
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

        if (_testCustomerOptions.IdentityUserId == Guid.Empty)
        {
            logger.LogWarning("Identity:TestCustomer:IdentityUserId is not configured. Sales test customer user seeding is skipped.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_testCustomerOptions.Email))
        {
            logger.LogWarning("Identity:TestCustomer:Email is not configured. Sales test customer user seeding is skipped.");
            return;
        }

        var user = await userManager.FindByEmailAsync(_testCustomerOptions.Email);
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(_testCustomerOptions.DefaultPassword))
            {
                logger.LogWarning(
                    "Identity:TestCustomer:DefaultPassword is not configured. Sales test customer user seeding is skipped.");
                return;
            }

            user = new AppUser
            {
                Id = _testCustomerOptions.IdentityUserId,
                UserName = _testCustomerOptions.Email,
                Email = _testCustomerOptions.Email,
                FullName = _testCustomerOptions.FullName,
                EmailConfirmed = true,
                LockoutEnabled = _testCustomerOptions.LockoutEnabled
            };

            var createResult = await userManager.CreateAsync(user, _testCustomerOptions.DefaultPassword);
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
