using Identity.Application.Services;
using Identity.Infrastructure.Options;
using Identity.Infrastructure.Services;

namespace Identity.Infrastructure.DependencyInjection;

public static class IdentityInfrastructureExtensions
{
    public static IServiceCollection AddIdentityInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityDbContextServices(configuration);
        services.Configure<ImportedCounterpartyIdentityOptions>(
            configuration.GetSection(ImportedCounterpartyIdentityOptions.SectionName));
        services.AddScoped<IIdentitySessionService, IdentitySessionService>();
        services.AddScoped<IImportedCounterpartyIdentityProvisioningService, ImportedCounterpartyIdentityProvisioningService>();
        return services;
    }
}
