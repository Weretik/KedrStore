using Infrastructure.Identity.Contracts;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Seeders;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.DependencyInjection;

public static class IdentityInfrastructureExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppIdentityDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("Default")));

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

