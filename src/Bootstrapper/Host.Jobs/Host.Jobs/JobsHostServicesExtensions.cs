using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Integrations.OneC.DependencyInjection;
using Catalog.Application.Integrations.OneC.Jobs;
using Catalog.Infrastructure.DependencyInjection;
using Identity.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Infrastructure.DependencyInjection;

namespace Host.Jobs;

public static class JobsHostServicesExtensions
{
    public static IServiceCollection AddJobsHostServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCatalogDbContextServices(configuration);
        services.AddCatalogServices(configuration);
        services.AddSalesInfrastructureServices(configuration);
        services.AddIdentityInfrastructureServices(configuration);

        services.AddOneCIntegrationServices();
        services.AddCatalogIntegrationOneCServices(configuration);

        services.AddScoped<SyncOneCFullJob>();
        services.AddScoped<SyncOneCPriceTypesJob>();
        services.AddScoped<SyncOneCCategoryJob>();
        services.AddScoped<SyncOneCProductDetailsJob>();
        services.AddScoped<SyncOneCStocksJob>();
        services.AddScoped<SyncOneCPricesJob>();

        return services;
    }
}
