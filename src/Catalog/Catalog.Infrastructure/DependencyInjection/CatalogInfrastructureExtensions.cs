using Catalog.Application.Features.Products.GetList.Options;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogInfrastructureExtensions
{
    public static IServiceCollection AddCatalogInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CatalogPricingOptions>(
            configuration.GetSection(CatalogPricingOptions.SectionName));

        services.AddCatalogDbContextServices(configuration);
        services.AddCatalogServices(configuration);

        return services;
    }
}
