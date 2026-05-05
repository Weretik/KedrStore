using Catalog.Infrastructure.DependencyInjection;
using Host.Api.DependencyInjection.ServiceRegistration.Options;

namespace Host.Api.DependencyInjection.ServiceRegistration;

public static class ModuleRegistrationsExtensions
{
    public static IServiceCollection AddModuleRegistrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityConfiguration(configuration);
        services.AddIdentityInfrastructureServices(configuration);

        services.AddInfrastructureServices(configuration);

        services.AddCatalogInfrastructureServices(configuration);
        services.AddCatalogIntegrationOneCServices(configuration);

        services.AddOneCIntegrationServices();

        return services;
    }
}
