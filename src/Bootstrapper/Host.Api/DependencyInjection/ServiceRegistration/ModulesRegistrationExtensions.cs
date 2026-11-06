using Content.Infrastructure.DependencyInjection;
using Host.Api.DependencyInjection.ServiceRegistration.Options;
using Media.Infrastructure.DependencyInjection;
using Sales.Infrastructure.DependencyInjection;
using Work.Infrastructure.DependencyInjection;

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
        services.AddSalesInfrastructureServices(configuration);
        services.AddContentInfrastructureServices(configuration);
        services.AddMediaInfrastructureServices(configuration);
        services.AddWorkInfrastructureServices(configuration);

        services.AddOneCIntegrationServices();

        return services;
    }
}
