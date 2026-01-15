using BuildingBlocks.Application.DependencyInjection;
using Catalog.Application;

namespace Host.Api.DependencyInjection.ServiceColltions.HostServices;

public static class MediatorExtensions
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
