using BuildingBlocks.Integrations.OneC.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Integrations.OneC.DependencyInjection;

public static class OneCIntegrationExtension
{
    public static IServiceCollection AddOneCIntegrationServices(this IServiceCollection services)
    {
        services.AddSingleton<OneCSoapClientFactory>();
        return services;
    }

}
