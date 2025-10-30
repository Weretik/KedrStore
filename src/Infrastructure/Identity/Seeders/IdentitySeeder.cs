using Domain.Identity.Constants;
using Infrastructure.Identity.Configuration;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Interfaces;
using Infrastructure.Identity.Utils;

namespace Infrastructure.Identity.Seeders;

public class IdentitySeeder(
    UserManager<AppUser> userManager,
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

        var password = Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD")
                       ?? SecurityUtils.GenerateSecurePassword();

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

        result = await userManager.AddToRoleAsync(user, AppRoles.Admin);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Помилка при призначенні ролі: {Errors}", errors);
        }

        if (string.IsNullOrWhiteSpace(_adminConfig.DefaultPassword) &&
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            logger.LogWarning("Згенеровано пароль адміністратора: {Password}", password);
        }

        logger.LogInformation("Користувач-адміністратор успішно створено: {Email}", user.Email);
    }
}
