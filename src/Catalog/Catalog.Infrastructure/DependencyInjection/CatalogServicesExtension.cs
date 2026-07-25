using Catalog.Application.Contracts.ClosedXML;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Orders.Create.Notifications;
using Catalog.Infrastructure.Exports;
using Catalog.Infrastructure.Notifications;
using Catalog.Infrastructure.Products;
using Catalog.Infrastructure.ReferenceData;

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

        return services;
    }

    public static IServiceCollection AddCatalogReferenceDataServices(this IServiceCollection services)
    {
        services.AddScoped<ICatalogReferenceDataReader, CatalogReferenceDataReader>();

        return services;
    }
}
