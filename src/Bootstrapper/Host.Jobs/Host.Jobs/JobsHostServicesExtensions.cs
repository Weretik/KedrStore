using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Integrations.OneC.DependencyInjection;
using Catalog.Infrastructure.DependencyInjection;
using Content.Infrastructure.DependencyInjection;
using Identity.Infrastructure.DependencyInjection;
using Media.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Infrastructure.DependencyInjection;
using Work.Infrastructure.DependencyInjection;

namespace Host.Jobs;

public static class JobsHostServicesExtensions
{
    public static IServiceCollection AddJobsHostServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTelegramServices(configuration);
        services.AddCatalogDbContextServices(configuration);
        services.AddCatalogContracts();
        services.AddProductMediaServices();
        services.AddSalesInfrastructureServices(configuration, includeCatalogReadServices: false);
        services.AddIdentityInfrastructureServices(configuration);
        services.AddContentInfrastructure(configuration);
        services.AddMediaInfrastructureServices(configuration);
        services.AddWorkInfrastructure(configuration);

        services.AddOneCIntegrationServices();
        services.AddCatalogIntegrationOneCServices(configuration);

        services.AddScoped<DatabaseMigrationJob>();

        services.AddCatalogJobs();

        return services;
    }
}
