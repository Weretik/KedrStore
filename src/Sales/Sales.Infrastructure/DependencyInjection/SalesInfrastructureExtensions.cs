namespace Sales.Infrastructure.DependencyInjection;

public static class SalesInfrastructureExtensions
{
    public static IServiceCollection AddSalesInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CatalogPricingOptions>(
            configuration.GetSection(CatalogPricingOptions.SectionName));

        services.AddScoped<ISalesCatalogPricePolicyProvider, DefaultSalesCatalogPricePolicyProvider>();
        services.AddScoped<ISalesCatalogProductReader, CatalogSalesCatalogProductReader>();

        return services;
    }
}
