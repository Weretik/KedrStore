using Catalog.Application.Integrations.OneC.Jobs;
using Catalog.Application.Integrations.OneC.Sync;

namespace Host.Api.DependencyInjection.ServiceRegistration;

public static class BackgroundJobRegistrationsExtensions
{
    public static IServiceCollection AddBackgroundJobRegistrations(this IServiceCollection services)
    {
        services.AddScoped<SyncOneCFullJob>();
        services.AddScoped<SyncOneCPriceTypesJob>();
        services.AddScoped<SyncOneCCategoryJob>();
        services.AddScoped<SyncOneCProductDetailsJob>();
        services.AddScoped<ProductTranslationImporter>();
        services.AddScoped<SyncOneCStocksJob>();
        services.AddScoped<SyncOneCPricesJob>();

        return services;
    }
}
