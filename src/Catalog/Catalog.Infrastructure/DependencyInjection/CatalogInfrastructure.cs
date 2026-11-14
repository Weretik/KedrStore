using Catalog.Contracts.Pricing;
using Catalog.Application.Abstractions.Integrations;
using Catalog.Infrastructure.Integrations.OneC;

namespace Catalog.Infrastructure.DependencyInjection;

/// <summary>
/// The one entry point of Catalog Infrastructure: binds the Application ports (Abstractions) and the Catalog
/// Contracts to their implementations, the DbContext, the 1C client, and the jobs. Called by <c>AddCatalogModule()</c>.
/// The smaller Add* methods stay public only for Host.Jobs and integration tests that need a subset.
/// </summary>
public static class CatalogInfrastructure
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CatalogPricingOptions>(
            configuration.GetSection(CatalogPricingOptions.SectionName));

        services.AddCatalogDbContextServices(configuration);
        services.AddCatalogServices(configuration);
        services.AddScoped<IXmlToJsonConverter, XmlToJsonConverter>();
        services.AddCatalogIntegrationOneCServices(configuration);
        services.AddCatalogJobs();

        return services;
    }
}
