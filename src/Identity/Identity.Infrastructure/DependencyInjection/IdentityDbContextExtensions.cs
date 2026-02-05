using Identity.Infrastructure.Contracts;
using Identity.Infrastructure.DataBase;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Seeders;

namespace Identity.Infrastructure.DependencyInjection;

public static class IdentityDbContextExtensions
{
    public static IServiceCollection AddIdentityDbContextServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppIdentityDbContext>());

        services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IIdentitySeeder, RoleSeeder>();
        services.AddScoped<IIdentitySeeder, IdentitySeeder>();

        services.AddScoped<IDatabaseMigrator, DbMigrator<AppIdentityDbContext>>();

        services.AddScoped<ISeeder, RoleSeeder>();
        services.AddScoped<ISeeder, IdentitySeeder>();

        return services;
    }
}
