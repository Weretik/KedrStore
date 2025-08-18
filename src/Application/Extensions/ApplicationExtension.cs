namespace Application.Extensions
{
    public static class ApplicationExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Регистрация Mediator
            services.AddMediator(options =>
            {
                options.Assemblies = [ typeof(ApplicationAssemblyMarker) ];
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.PipelineBehaviors =
                [
                    typeof(UnhandledExceptionBehavior<,>),
                    typeof(RequestLoggingBehavior<,>),
                    typeof(ValidationToResultBehavior<>),
                    typeof(ValidationToResultGenericBehavior<,>),
                    typeof(PerformanceBehavior<,>),
                    typeof(DomainEventDispatcherBehavior<,>)
                ];
            });

            // Регистрация FluentValidation
            services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly, includeInternalTypes: true);

            return services;
        }

    }
}
