using BuildingBlocks.Application;
using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Application.Validation;
using Catalog.Application;
using Content.Application;
using Identity.Application;
using Media.Application;
using Sales.Application;
using Scheduling.Application;
using Work.Application;
namespace Host.Api.Application;

public static class MediatorRegistrationExtensions
{
    public static IServiceCollection AddMediatorPipeline(
        this IServiceCollection services)
    {
        UkrainianValidationMessages.Use();
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
                typeof(SchedulingApplicationAssemblyMarker).Assembly,
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
