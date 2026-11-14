using Catalog.Application.Jobs.Attributes;
using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Integrations.OneC.DependencyInjection;
using Catalog.Application.Integrations.OneC.Jobs;
using Catalog.Application.Integrations.OneC.Sync;
using Catalog.Application.Jobs.Slugs;
using Catalog.Infrastructure.DependencyInjection;
using Content.Infrastructure.DependencyInjection;
using Identity.Infrastructure.DependencyInjection;
using Media.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Infrastructure.DependencyInjection;
using Sales.Infrastructure.Integrations.OneC.Jobs;
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
        services.AddContentInfrastructureServices(configuration);
        services.AddMediaInfrastructureServices(configuration);
        services.AddWorkInfrastructureServices(configuration);

        services.AddOneCIntegrationServices();
        services.AddCatalogIntegrationOneCServices(configuration);

        services.AddScoped<DatabaseMigrationJob>();

        services.AddScoped<SyncOneCFullJob>();
        services.AddScoped<SyncOneCPriceTypesJob>();
        services.AddScoped<SyncOneCCategoryJob>();
        services.AddScoped<SyncOneCProductDetailsJob>();
        services.AddScoped<ProductTranslationImporter>();
        services.AddScoped<MigrateLegacySlugsJob>();
        services.AddScoped<ImportCatalogAttributesJob>();
        services.AddScoped<SyncOneCStocksJob>();
        services.AddScoped<SyncOneCPricesJob>();
        services.AddScoped<SyncOneCOrdersJob>();

        return services;
    }
}
