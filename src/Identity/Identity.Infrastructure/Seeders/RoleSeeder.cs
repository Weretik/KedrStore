using Identity.Domain.Authorization;
using Identity.Infrastructure.Contracts;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Seeders;

public class RoleSeeder(RoleManager<AppRole> roleManager, ILogger<RoleSeeder> logger) : IIdentitySeeder
{
    public async Task SeedAsync(IServiceProvider _, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roles = new List<AppRole>
        {
            new() { Name = RoleNames.Admin, Description = "Адміністратор системи", Scope = "system", IsSystemRole = true, AccessLevel = 100 },
            new() { Name = RoleNames.Manager, Description = "Контент-менеджер", Scope = "content", IsSystemRole = true, AccessLevel = 50 },
            new() { Name = RoleNames.User, Description = "Звичайний користувач", Scope = "user", IsSystemRole = true, AccessLevel = 10 }
        };

        roles.Add(new AppRole
        {
            Name = RoleNames.Counterparty,
            Description = "Контрагент",
            Scope = "counterparty",
            IsSystemRole = true,
            AccessLevel = 10
        });

        foreach (var role in roles)
        {
            if (string.IsNullOrWhiteSpace(role.Name))
            {
                logger.LogError("Identity role seed skipped because role name is empty.");
                continue;
            }

            var exists = await roleManager.FindByNameAsync(role.Name);
            if (exists != null) continue;

            role.NormalizedName = role.Name.ToUpperInvariant();

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
