using Identity.Domain.Authorization;
using Identity.Infrastructure.Configuration;
using Identity.Infrastructure.Contracts;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Seeders;

public class IdentitySeeder(
    UserManager<AppUser> userManager,
    IConfiguration configuration,
    IOptions<AdminUserConfig> adminOptions,
    ILogger<IdentitySeeder> logger)
    : IIdentitySeeder
{
    private readonly AdminUserConfig _adminConfig = adminOptions.Value;

    public async Task SeedAsync(IServiceProvider _, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await SeedAdminUserAsync(cancellationToken);
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingAdmin = await userManager.FindByEmailAsync(_adminConfig.Email);
        if (existingAdmin != null)
        {
            logger.LogInformation("Адміністратор вже існує: {Email}", _adminConfig.Email);
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
            logger.LogError("Помилка при створенні адміністратора: {Errors}", errors);
            return;
        }

        result = await userManager.AddToRoleAsync(user, RoleNames.Admin);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Помилка при призначенні ролі: {Errors}", errors);
        }

        logger.LogInformation("Користувач-адміністратор успішно створено: {Email}", user.Email);
    }
}
