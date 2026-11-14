using Identity.Contracts.Authorization;
using Identity.Infrastructure.Seeding;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Seeding;

public class RoleSeeder(RoleManager<AppRole> roleManager, ILogger<RoleSeeder> logger) : IIdentitySeeder
{
    public async Task SeedAsync(IServiceProvider _, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roles = new List<AppRole>
        {
            new() { Name = RoleNames.Admin, Description = "Адміністратор системи", Scope = "system", IsSystemRole = true, AccessLevel = 100 },
            new() { Name = RoleNames.Seo, Description = "SEO-спеціаліст", Scope = "content", IsSystemRole = true, AccessLevel = 70 },
            new() { Name = RoleNames.Manager, Description = "Контент-менеджер", Scope = "content", IsSystemRole = true, AccessLevel = 50 },
            new() { Name = RoleNames.User, Description = "Звичайний користувач", Scope = "user", IsSystemRole = true, AccessLevel = 10 }
        };

        foreach (var role in roles)
        {
            var roleName = role.Name ?? throw new InvalidOperationException("Seed roles must have a name.");
            var exists = await roleManager.FindByNameAsync(roleName);
            if (exists != null) continue;

            role.NormalizedName = roleName.ToUpperInvariant();

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Помилка при створенні ролі {Role}: {Errors}", role.Name, errors);
            }
            else
            {
                logger.LogInformation("Роль {Role} успішно створена", role.Name);
            }
        }
    }
}
