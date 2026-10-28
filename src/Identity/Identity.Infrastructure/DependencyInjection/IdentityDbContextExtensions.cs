using Identity.Infrastructure.Contracts;
using Identity.Infrastructure.DataBase;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Options;
using Identity.Infrastructure.Security;
using Identity.Infrastructure.Seeders;
using Microsoft.AspNetCore.DataProtection;

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

        // Keys live in PostgreSQL so tokens survive deploys and work on every Cloud Run instance.
        services
            .AddDataProtection()
            .PersistKeysToDbContext<AppIdentityDbContext>()
            .SetApplicationName("KedrStore");

        services.AddIdentityAuthentication(configuration);

        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<AppRole>()
            .AddSignInManager()
            .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders()
            .AddApiEndpoints();

        services.AddScoped<IIdentitySeeder, RoleSeeder>();
        services.AddScoped<IIdentitySeeder, IdentitySeeder>();

        services.AddScoped<IDatabaseMigrator, DbMigrator<AppIdentityDbContext>>();

        services.AddScoped<ISeeder, RoleSeeder>();
        services.AddScoped<ISeeder, IdentitySeeder>();

        return services;
    }
}
