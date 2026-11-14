using Catalog.Contracts.Pricing;
using Catalog.Application.Abstractions.Integrations;
using Catalog.Infrastructure.Integrations.OneC;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Categories;

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
        services.AddScoped<IXmlToJsonConverter, XmlToJsonConverter>();

        return services;
    }
}
