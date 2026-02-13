namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogInfrastructureExtensions
{
    public static IServiceCollection AddCatalogInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCatalogDbContextServices(configuration);
        services.AddCatalogServices(configuration);

        return services;
    }
}
