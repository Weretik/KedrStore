using Catalog.Application.Contracts.ClosedXML;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Orders.Create.Notifications;
using Catalog.Infrastructure.Exports;
using Catalog.Infrastructure.Notifications;
using Catalog.Infrastructure.Products;
using Catalog.Infrastructure.ReferenceData;
using Catalog.Application.Contracts.Integrations;
using Catalog.Application.Jobs.ProductPhotos;
using Catalog.Infrastructure.Integrations.ProductPhotos;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogServicesExtension
{
    public static IServiceCollection AddCatalogServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITelegramNotifier, TelegramBotNotifier>();
        services.AddScoped<IOrderExcelExporter, OrderExcelExporter>();
        services.AddScoped<ICatalogProductListReader, CatalogProductListReader>();
        services.AddCatalogReferenceDataServices();
        services.AddProductPhotoCheckServices(configuration);

        return services;
    }

    public static IServiceCollection AddProductPhotoCheckServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ProductPhotoCheckOptions>(
            configuration.GetSection(ProductPhotoCheckOptions.SectionName));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<CheckProductPhotosJob>();
        services.AddHttpClient<IProductPhotoProbe, ProductPhotoProbe>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false
            });

        return services;
    }

    public static IServiceCollection AddCatalogReferenceDataServices(this IServiceCollection services)
    {
        services.AddScoped<ICatalogReferenceDataReader, CatalogReferenceDataReader>();

        return services;
    }
}
