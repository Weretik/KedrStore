namespace Catalog.Application.DependencyInjection;

public static class CatalogDependencyInjection
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(CatalogApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        return services;
    }
}
