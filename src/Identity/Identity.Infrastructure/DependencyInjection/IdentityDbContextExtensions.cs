using Identity.Infrastructure.Contracts;
using Identity.Infrastructure.DataBase;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Options;
using Identity.Infrastructure.Security;
using Identity.Infrastructure.Seeders;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace Identity.Infrastructure.DependencyInjection;

public static class IdentityDbContextExtensions
{
    public static IServiceCollection AddIdentityDbContextServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var sessionSecurityOptions = configuration
            .GetSection(IdentitySessionSecurityOptions.SectionName)
            .Get<IdentitySessionSecurityOptions>() ?? new IdentitySessionSecurityOptions();

        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppIdentityDbContext>());

        services
            .AddAuthentication(IdentityConstants.BearerScheme)
            .AddBearerToken(IdentityConstants.BearerScheme, options =>
            {
                options.BearerTokenExpiration = TimeSpan.FromMinutes(sessionSecurityOptions.AccessTokenLifetimeMinutes);
                options.RefreshTokenExpiration = TimeSpan.FromDays(sessionSecurityOptions.RefreshAbsoluteLifetimeDays);
            });

        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
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
