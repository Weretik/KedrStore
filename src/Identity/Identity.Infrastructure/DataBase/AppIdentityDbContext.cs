using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.DataBase;

public class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
    : IdentityDbContext<AppUser, AppRole, Guid, IdentityUserClaim<Guid>, AppUserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>(options)
{
    public DbSet<AppRefreshSession> RefreshSessions => Set<AppRefreshSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(
            typeof(AppIdentityDbContext).Assembly,
            type => type.Namespace?.StartsWith("Identity.Infrastructure.Configuration") ?? false);
    }
}
