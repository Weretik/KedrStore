using Identity.Application.Services;
using Identity.Infrastructure.Avatars;
using Identity.Infrastructure.Options;
using Identity.Infrastructure.Services.Profile;
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
