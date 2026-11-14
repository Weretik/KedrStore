using Catalog.Application.Abstractions.Integrations;
using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Infrastructure.Integrations.OneC;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogIntegrationOneCExtension
{
    public static IServiceCollection AddCatalogIntegrationOneCServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RootCategoryId>(configuration.GetSection("OneC"));
        services.Configure<CategoryPresentationOptions>(configuration.GetSection("OneC:CategoryPresentation"));
        services.AddScoped<IOneCClient, OneCClient>();
        return services;
    }
}
