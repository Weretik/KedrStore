using Infrastructure.Identity.Entities;

namespace Infrastructure.Identity;

public class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
    : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(
            typeof(AppIdentityDbContext).Assembly,
            type => type.Namespace?.StartsWith("Infrastructure.Identity") ?? false);
    }
}
