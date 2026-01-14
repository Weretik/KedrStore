using Catalog.Application.Integrations.OneC.Client;
using Catalog.Application.Integrations.OneC.Contracts;

namespace Catalog.Application.DependencyInjection;

public static class CatalogDependencyInjection
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(CatalogApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        services.AddScoped<IOneCClient, OneCClient>();

        return services;
    }
}
