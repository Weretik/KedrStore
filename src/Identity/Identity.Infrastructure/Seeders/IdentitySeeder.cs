using Identity.Domain.Authorization;
using Identity.Infrastructure.Contracts;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Options;

namespace Identity.Infrastructure.Seeders;

public class IdentitySeeder(
    UserManager<AppUser> userManager,
    IConfiguration configuration,
    IOptions<AdminUserOptions> adminOptions,
    ILogger<IdentitySeeder> logger)
    : IIdentitySeeder
{
    private readonly AdminUserOptions _adminOptions = adminOptions.Value;

    public async Task SeedAsync(IServiceProvider _, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await SeedAdminUserAsync(cancellationToken);
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
}
