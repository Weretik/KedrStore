using Sales.Application.Abstractions.Customers;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Integrations.OneC;
using Sales.Infrastructure.Customers;
using Sales.Infrastructure.Integrations.Excel;
using Sales.Infrastructure.Integrations.OneC;
using Sales.Infrastructure.Integrations.OneC.Jobs;
using Sales.Infrastructure.Integrations.Telegram;
using Sales.Infrastructure.Orders;
using Sales.Infrastructure.QuickOrders;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Application.Abstractions.Workspace.Orders;
using Sales.Application.Abstractions.Workspace.Drafts;
using Sales.Infrastructure.Integrations.OneC.Counterparties;
using Sales.Infrastructure.Integrations.OneC.Orders;
using Sales.Application.Abstractions.Orders.Sync;
using Sales.Application.Abstractions.Workspace.Overview;
using Sales.Application.Abstractions.Workspace.Pricing;

namespace Sales.Infrastructure.DependencyInjection;

public static class SalesInfrastructure
{
    public static IServiceCollection AddSalesInfrastructure(
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
        services.AddOutbox<SalesDbContext>(SalesDbContext.Schema);
        services.AddScoped<IQuickOrderStore, QuickOrderStore>();
        services.AddOutboxHandler<QuickOrderPlaced, QuickOrderTelegramSender>(QuickOrderPlaced.TypeName);
        services.AddScoped<IReadSalesDbContext>(sp => sp.GetRequiredService<SalesDbContext>());
        services.AddScoped(typeof(ISalesRepository<>), typeof(SalesEfRepository<>));
        services.AddScoped<IOrderCreationStore, OrderCreationStore>();
        services.AddScoped<CustomerPriceResolver>();
        services.AddScoped<AdminWorkspacePricing>();
        services.AddScoped<IWorkspacePricing>(sp => sp.GetRequiredService<AdminWorkspacePricing>());
        services.AddScoped<IWorkspaceOrderReader, WorkspaceOrderReader>();
        services.AddScoped<IWorkspaceOverviewReader, WorkspaceOverviewReader>();
        services.AddScoped<IWorkspaceOrderCreator, WorkspaceOrderCreator>();
        services.AddScoped<WorkspaceDraftDetails>();
        services.AddScoped<IWorkspaceDraftReader, WorkspaceDraftReader>();
        services.AddScoped<IWorkspaceDraftWriter, WorkspaceDraftWriter>();
        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
        services.AddScoped<IOrderReadService, OrderReadService>();
        services.AddScoped<ICustomerReadService, CustomerReadService>();
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
        services.AddScoped<StaleCounterpartyRemoval>();
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
