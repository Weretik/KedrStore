using Identity.Application.Services;
using Identity.Infrastructure.Options;
using Identity.Infrastructure.Services.Sessions;
using Identity.Infrastructure.Services.Users;

namespace Identity.Infrastructure.DependencyInjection;

public static class IdentityInfrastructureExtensions
{
    public static IServiceCollection AddIdentityInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityDbContextServices(configuration);
        // Bound here so the migration job, which runs IdentitySeeder, gets the administrator settings too.
        services.Configure<AdminUserOptions>(configuration.GetSection(AdminUserOptions.SectionName));
        services.Configure<ImportedCounterpartyIdentityOptions>(
            configuration.GetSection(ImportedCounterpartyIdentityOptions.SectionName));
        services.AddScoped<RefreshSessionStore>();
        services.AddScoped<IdentityBearerTokenIssuer>();
        services.AddScoped<IIdentitySessionService, IdentitySessionService>();
        services.AddScoped<ICookieSessionService, CookieSessionService>();
        services.AddScoped<IStaffAccountService, StaffAccountService>();
        services.AddScoped<IImportedCounterpartyIdentityProvisioningService, ImportedCounterpartyIdentityProvisioningService>();
        return services;
    }
}
