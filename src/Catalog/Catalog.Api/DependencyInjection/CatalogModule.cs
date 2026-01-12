using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Api.DependencyInjection;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogApi(this IServiceCollection services)
    {
        return services;
    }
}
