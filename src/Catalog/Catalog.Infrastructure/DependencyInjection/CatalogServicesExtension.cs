using Catalog.Application.Contracts.ClosedXML;
using Catalog.Application.Features.Orders.Create.Notifications;
using Catalog.Infrastructure.Exports;
using Catalog.Infrastructure.Notifications;


namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogServicesExtension
{
    public static IServiceCollection AddCatalogServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITelegramNotifier, TelegramBotNotifier>();
        services.AddScoped<IOrderExcelExporter, OrderExcelExporter>();

        return services;
    }
}
