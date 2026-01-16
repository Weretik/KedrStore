using Catalog.Application.Integrations.OneC.Client;
using Catalog.Application.Integrations.OneC.Contracts;

namespace Catalog.Application.DependencyInjection;

public static class CatalogDependencyExtension
{
    public static IServiceCollection AddCatalogApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOneCClient, OneCClient>();

        return services;
    }
}
