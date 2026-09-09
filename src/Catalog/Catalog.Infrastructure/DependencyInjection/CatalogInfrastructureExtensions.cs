using Catalog.Contracts.Pricing;
using Catalog.Application.Contracts.Json;
using Catalog.Infrastructure.Converters;

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
        services.AddScoped<IXmlToJsonConvector, XmlToJsonConvector>();

        return services;
    }
}
