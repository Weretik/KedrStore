using Sales.Application.Integrations.OneC.Contracts;
using Sales.Infrastructure.Integrations.OneC;
using Sales.Infrastructure.Integrations.OneC.Jobs;
using Sales.Infrastructure.Integrations.OneC.Services;

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

        services.AddDbContext<SalesDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IReadSalesDbContext>(sp => sp.GetRequiredService<SalesDbContext>());
        services.AddScoped(typeof(ISalesRepository<>), typeof(SalesEfRepository<>));
        services.AddScoped<IDatabaseMigrator, DbMigrator<SalesDbContext>>();

        services.AddScoped<ISalesOneCReadClient, SalesOneCReadClient>();
        services.AddScoped<CounterpartyContactNormalizer>();
        services.AddScoped<OneCCounterpartiesSyncService>();
        services.AddScoped<OneCCounterpartyCategoryPriceTypesSyncService>();
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
