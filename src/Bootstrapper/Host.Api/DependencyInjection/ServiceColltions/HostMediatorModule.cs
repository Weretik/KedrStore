using BuildingBlocks.Application.DependencyInjection;
using Catalog.Application;

namespace Host.Api.DependencyInjection.ServiceColltions;

public static class HostMediatorModule
{
    public static IServiceCollection AddMediatorPipeline(
        this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;

            options.Assemblies =
            [
                typeof(CatalogApplicationAssemblyMarker),
                // typeof(OrderingApplicationAssemblyMarker),
            ];

            options.PipelineBehaviors = MediatorPipeline.PipelineBehaviors;
        });

        return services;
    }
}
