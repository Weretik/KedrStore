using BuildingBlocks.Application.Behaviors;

namespace BuildingBlocks.Application.DependencyInjection
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            services.AddMediator(options =>
            {
                options.Assemblies = assemblies
                    .Select(static a => (AssemblyReference)a)
                    .ToArray();

                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.PipelineBehaviors =
                [
                    typeof(RequestLoggingBehavior<,>),
                    typeof(PerformanceBehavior<,>),
                    typeof(ValidationBehavior<,>),
                    typeof(ExceptionBehavior<,>),
                    typeof(DomainEventDispatcherBehavior<,>)
                ];
            });

            return services;
        }
    }
}
