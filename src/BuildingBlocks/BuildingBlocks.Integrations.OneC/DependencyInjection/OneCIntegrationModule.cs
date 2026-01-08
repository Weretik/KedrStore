using BuildingBlocks.Application.Integrations.OneC.Contracts;
using BuildingBlocks.Integrations.OneC.Client;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Integrations.OneC.DependencyInjection;

public static class OneCIntegrationModule
{
    public static IServiceCollection AddOneCIntegration(this IServiceCollection services)
    {
        services.AddSingleton<OneCSoapClientFactory>();
        services.AddScoped<IOneCClient, OneCClient>();
        return services;
    }

}
