using Catalog.Application.Integrations.OneC.Jobs;
using Catalog.Application.Integrations.OneC.Sync;
using Catalog.Application.Jobs.Attributes;
using Catalog.Application.Jobs.Slugs;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogJobsExtensions
{
    /// <summary>Catalog jobs that Host.Jobs runs by name and the API can start on demand.</summary>
    public static IServiceCollection AddCatalogJobs(this IServiceCollection services)
    {
        services.AddScoped<SyncOneCFullJob>();
        services.AddScoped<SyncOneCPriceTypesJob>();
        services.AddScoped<SyncOneCCategoryJob>();
        services.AddScoped<SyncOneCProductDetailsJob>();
        services.AddScoped<ProductTranslationImporter>();
        services.AddScoped<SyncOneCStocksJob>();
        services.AddScoped<SyncOneCPricesJob>();
        services.AddScoped<MigrateLegacySlugsJob>();
        services.AddScoped<ImportCatalogAttributesJob>();

        return services;
    }
}
