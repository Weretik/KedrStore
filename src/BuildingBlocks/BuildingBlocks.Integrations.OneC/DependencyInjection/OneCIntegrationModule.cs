using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Integrations.OneC.DependencyInjection;

public static class OneCIntegrationModule
{
    public static IServiceCollection AddOneCIntegration(this IServiceCollection services)
    {
        services.AddSingleton<OneCSoapClientFactory>();
        return services;
    }

}
