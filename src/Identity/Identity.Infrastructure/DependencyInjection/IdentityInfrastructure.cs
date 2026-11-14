using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;
using Identity.Contracts.Users;
using Identity.Infrastructure.Avatars;
using Identity.Infrastructure.Avatars;
using Identity.Infrastructure.Seeding;
using Identity.Infrastructure.Sessions;
using Identity.Infrastructure.Users;
using Identity.Infrastructure.Profile;
using Identity.Infrastructure.Sessions;
using Identity.Infrastructure.Users;

namespace Identity.Infrastructure.DependencyInjection;

public static class IdentityInfrastructure
{
    public static IServiceCollection AddIdentityInfrastructure(
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
        services.AddScoped<ManagedStaffResolver>();
        services.AddScoped<AccountEmailWriter>();
        services.AddScoped<IStaffProfileEditService, StaffProfileEditService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddAvatarServices(configuration);
        return services;
    }

    private static void AddAvatarServices(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AvatarStorageOptions.SectionName);
        services.Configure<AvatarStorageOptions>(section);
        services.AddSingleton<AvatarImageProcessor>();
        services.AddSingleton<AvatarUrlBuilder>();
        services.AddScoped<AvatarService>();

        // Without CDN credentials (local runs, tests) uploads answer 503 instead of failing at startup.
        if (section.Get<AvatarStorageOptions>()?.IsConfigured == true)
        {
            services.AddSingleton<IAvatarStorage, FtpAvatarStorage>();
        }
        else
        {
            services.AddSingleton<IAvatarStorage, UnconfiguredAvatarStorage>();
        }
    }
}
