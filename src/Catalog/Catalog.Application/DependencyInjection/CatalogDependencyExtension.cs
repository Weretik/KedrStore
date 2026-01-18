using Catalog.Application.Integrations.OneC.Client;
using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.Options;

namespace Catalog.Application.DependencyInjection;

public static class CatalogDependencyExtension
{
    public static IServiceCollection AddCatalogApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RootCategoryId>(configuration.GetSection("OneC"));
        services.AddScoped<IOneCClient, OneCClient>();
        return services;
    }
}
