using BuildingBlocks.Application.Outbox;
using Sales.Application.Abstractions.Customers;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Integrations.OneC;
using Sales.Infrastructure.Customers;
using Sales.Infrastructure.Integrations.Excel;
using Sales.Infrastructure.Integrations.OneC;
using Sales.Application.Features.OneCSync.Counterparties;
using Sales.Application.Features.OneCSync.Jobs;
using Sales.Infrastructure.Integrations.Telegram;
using Sales.Infrastructure.Orders;
using Sales.Infrastructure.QuickOrders;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Infrastructure.Persistence.QuickOrders;
using Sales.Application.Abstractions.Workspace.Orders;
using Sales.Application.Abstractions.Workspace.Drafts;
using Sales.Application.Features.Orders.OneCDelivery.GiveUp;
using Sales.Application.Features.Orders.OneCDelivery.Messages;
using Sales.Application.Features.Orders.OneCDelivery.Send;
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

        services.AddOneCSendingWindow(configuration);

        services.AddDbContext<SalesDbContext>(options => options.UseNpgsql(connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", SalesDbContext.Schema)));
        services.AddOutbox<SalesDbContext>(SalesDbContext.Schema);
        // Spec sales/009: site orders, their Telegram delivery and give-up, and the Admin reads.
        services.AddScoped<IQuickOrderStore, QuickOrderStore>();
        services.AddScoped<IQuickOrderReadService, QuickOrderReadService>();
        services.AddOutboxHandler<QuickOrderPlaced, QuickOrderTelegramSender>(QuickOrderPlaced.TypeName);
        services.AddOutboxGiveUpHandler<QuickOrderPlaced, QuickOrderTelegramGiveUpHandler>();
        services.AddSiteOrdersToOneC(configuration);
        // Platform spec 006, stage 2: orders to 1C, their give-up signal, and the removed customers' Identity users.
        services.AddScoped<IOrderOneCDeliveryStore, OrderOneCDeliveryStore>();
        services.AddOutboxHandler<OrderPlaced, SendOrderToOneCHandler>(OrderPlaced.TypeName, OutboxLanes.OneCOrders);
        services.AddOutboxGiveUpHandler<OrderPlaced, OrderOneCGiveUpHandler>();
        services.AddOutboxHandler<OrderOneCDeliveryFailed, OrderOneCDeliveryFailedHandler>(OrderOneCDeliveryFailed.TypeName);
        services.AddOutboxHandler<CounterpartyRemoved, RemovedCounterpartyIdentityDeleter>(CounterpartyRemoved.TypeName);
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
        services.AddScoped<ICounterpartySyncStore, CounterpartySyncStore>();
        services.AddScoped<ICustomerLogins, CustomerLogins>();
        services.AddScoped<ICatalogReferenceIds, CatalogReferenceIds>();
        services.AddScoped<SyncOneCCounterpartiesJob>();
        services.AddScoped<SyncOneCCounterpartyCategoryPriceTypesJob>();
        services.AddScoped<SyncOneCSalesCustomersFullJob>();
        if (includeCatalogReadServices)
        {
            services.AddScoped<IPricePolicyProvider, DefaultPricePolicyProvider>();
            services.AddScoped<ICatalogProductReader, CatalogProductReader>();
        }

        return services;
    }
}
