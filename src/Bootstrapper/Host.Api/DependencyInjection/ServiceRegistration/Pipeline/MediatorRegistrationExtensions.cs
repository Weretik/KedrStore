using BuildingBlocks.Application;
using BuildingBlocks.Application.Behaviors;
using Catalog.Application;
using Content.Application;
using Identity.Application;
using Media.Application;
using Sales.Application;
using Work.Application;

namespace Host.Api.DependencyInjection.ServiceRegistration.Pipeline;

public static class MediatorRegistrationExtensions
{
    public static IServiceCollection AddMediatorPipeline(
        this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;

            options.Assemblies =
            [
                typeof(CatalogApplicationAssemblyMarker).Assembly,
                typeof(IdentityApplicationAssemblyMarker).Assembly,
                typeof(SalesApplicationAssemblyMarker).Assembly,
                typeof(ContentApplicationAssemblyMarker).Assembly,
                typeof(MediaApplicationAssemblyMarker).Assembly,
                typeof(WorkApplicationAssemblyMarker).Assembly,
                typeof(ApplicationAssemblyMarker).Assembly
            ];

            options.PipelineBehaviors =
            [
                typeof(RequestLoggingBehavior<,>),
                typeof(ExceptionBehavior<,>),
                typeof(PerformanceBehavior<,>),
                typeof(ValidationBehavior<,>),
                typeof(DomainEventDispatcherBehavior<,>)
            ];
        });

        return services;
    }
}
