using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Integrations.OneC.DependencyInjection;
using Catalog.Api.DependencyInjection;
using Content.Api.DependencyInjection;
using Identity.Api.DependencyInjection;
using Media.Api.DependencyInjection;
using Sales.Api.DependencyInjection;
using Work.Api.DependencyInjection;

namespace Host.Api.DependencyInjection.ServiceRegistration;

/// <summary>The list of modules. Each module registers its own layers in Add&lt;Module&gt;Module().</summary>
public static class ModuleRegistrationsExtensions
{
    public static IServiceCollection AddModuleRegistrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructureServices(configuration);
        services.AddOneCIntegrationServices();

        services.AddIdentityModule(configuration);
        services.AddCatalogModule(configuration);
        services.AddSalesModule(configuration);
        services.AddContentModule(configuration);
        services.AddMediaModule(configuration);
        services.AddWorkModule(configuration);

        return services;
    }
}
