using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Infrastructure.Integrations.OneC.Client;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogIntegrationOneCExtension
{
    public static IServiceCollection AddCatalogIntegrationOneCServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RootCategoryId>(configuration.GetSection("OneC"));
        services.AddScoped<IOneCClient, OneCClient>();
        return services;
    }
}
