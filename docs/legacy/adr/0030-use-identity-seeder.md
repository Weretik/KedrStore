# ADR 0030: Use IdentitySeeder

## Date
2025-06-17

## Status
Accepted

## Context
The IdentitySeeder is a component responsible for initializing the identity system with default users, roles, and permissions. It ensures that the application has a predefined administrator account and necessary roles upon startup, simplifying the setup process and ensuring consistency across environments.

## Decision
We decided to use IdentitySeeder in the project to:

1. Automate the initialization of the identity system with default users and roles.
2. Ensure consistency in identity setup across development, staging, and production environments.
3. Simplify the onboarding process for new developers by providing a predefined administrator account.
4. Align with best practices for secure and scalable identity management.

## Consequences
### Positive
1. Automates the creation of default users and roles, reducing manual setup.
2. Ensures consistency in identity configuration across environments.
3. Simplifies onboarding for new developers by providing a predefined administrator account.
4. Improves security by enforcing default roles and permissions.

### Negative
1. Adds complexity to the application startup process.
2. Requires careful management of default credentials to avoid security risks.
3. May lead to issues if the seeder is not properly configured or executed.

## Example
IdentitySeeder is implemented as follows:

**IdentitySeeder.cs**:
```csharp
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
            logger.LogInformation("Адміністратор уже існує: {Email}", _adminConfig.Email);
            return;
        }

        var password = _adminConfig.DefaultPassword ?? SecurityUtils.GenerateSecurePassword();

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
```
