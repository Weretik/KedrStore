using Sales.Application.Integrations.OneC.Contracts;
using Sales.Application.Contracts.Orders;
using Sales.Infrastructure.Integrations.OneC;
using Sales.Infrastructure.Integrations.OneC.Jobs;
using Sales.Infrastructure.Integrations.OneC.Services;
using Sales.Infrastructure.Exports;
using Sales.Infrastructure.Notifications;
using Sales.Infrastructure.Orders;

namespace Sales.Infrastructure.DependencyInjection;

public static class SalesInfrastructureExtensions
{
    public static IServiceCollection AddSalesInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool includeCatalogReadServices = true)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        services.Configure<CatalogPricingOptions>(
            configuration.GetSection(CatalogPricingOptions.SectionName));

        services.AddOneCOrderSyncConfiguration(configuration);

        services.AddDbContext<SalesDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IReadSalesDbContext>(sp => sp.GetRequiredService<SalesDbContext>());
        services.AddScoped(typeof(ISalesRepository<>), typeof(SalesEfRepository<>));
        services.AddScoped<IOrderCreationStore, OrderCreationStore>();
        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
        services.AddScoped<IOrderSyncStatusReader, OrderSyncStatusReader>();
        services.AddScoped<IOrderSyncRetryStore, OrderSyncRetryStore>();
        services.AddScoped<IOrderProductReader, OrderProductReader>();
        services.AddScoped<IOrderDeliveryFailureExporter, OrderDeliveryFailureExporter>();
        services.AddScoped<IDeadLetterNotifier, DeadLetterTelegramNotifier>();
        services.AddScoped<IDatabaseMigrator, DbMigrator<SalesDbContext>>();

        services.AddScoped<ISalesOneCReadClient, SalesOneCReadClient>();
        services.AddScoped<IOneCSiteRequestSender, OneCSiteRequestSender>();
        services.AddScoped<ISalesOneCWriteClient, SalesOneCWriteClient>();
        services.AddScoped<CounterpartyContactNormalizer>();
        services.AddScoped<OneCCounterpartiesSyncService>();
        services.AddScoped<OneCCounterpartyCategoryPriceTypesSyncService>();
        services.AddScoped<SyncOneCCounterpartiesJob>();
        services.AddScoped<SyncOneCCounterpartyCategoryPriceTypesJob>();
        services.AddScoped<SyncOneCSalesCustomersFullJob>();
        services.AddScoped<SyncOneCOrdersService>();
        services.AddScoped<SyncOneCOrdersJob>();
        services.AddScoped<DeadLetterNotificationService>();
        if (includeCatalogReadServices)
        {
            services.AddScoped<IPricePolicyProvider, DefaultPricePolicyProvider>();
            services.AddScoped<ICatalogProductReader, CatalogProductReader>();
        }

        return services;
    }
}
