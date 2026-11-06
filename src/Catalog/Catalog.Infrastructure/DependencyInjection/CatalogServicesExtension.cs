using Catalog.Application.Contracts.ClosedXML;
using Catalog.Application.Contracts.Content;
using Catalog.Infrastructure.Content;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Orders.Create.Notifications;
using Catalog.Infrastructure.Exports;
using Catalog.Infrastructure.Notifications;
using Catalog.Infrastructure.Products;
using Catalog.Infrastructure.ReferenceData;
using Catalog.Application.Contracts.Media;
using Catalog.Application.Features.ProductMedia.FileChanges;
using Catalog.Application.Jobs.ProductMedia;
using Catalog.Infrastructure.MediaLibrary;
using Media.Contracts.Files;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogServicesExtension
{
    public static IServiceCollection AddCatalogServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITelegramNotifier, TelegramBotNotifier>();
        services.AddScoped<IOrderExcelExporter, OrderExcelExporter>();
        services.AddScoped<ICatalogProductListReader, CatalogProductListReader>();
        services.AddSingleton<IHtmlSanitizerService, HtmlSanitizerService>();
        services.AddCatalogReferenceDataServices();
        services.AddProductMediaServices();

        return services;
    }

    /// <summary>
    /// Product photos and schemes from the media library: the port to Media, the listener for file replace and
    /// delete, and the link job. Needs the Media infrastructure registered too.
    /// </summary>
    public static IServiceCollection AddProductMediaServices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IProductMediaLibrary, ProductMediaLibrary>();
        services.AddScoped<IAttributeSwatchStorage, AttributeSwatchStorage>();
        services.AddScoped<ProductMediaFileChanges>();
        services.AddScoped<IMediaFileChangeListener, CatalogMediaFileChangeListener>();
        services.AddScoped<LinkProductMediaJob>();
        services.AddScoped<ArrangeProductMediaFoldersJob>();

        return services;
    }

    public static IServiceCollection AddCatalogReferenceDataServices(this IServiceCollection services)
    {
        services.AddScoped<ICatalogReferenceDataReader, CatalogReferenceDataReader>();

        return services;
    }
}
