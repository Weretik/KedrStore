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
                    typeof(RequestLoggingBehavior<,>),
                    typeof(PerformanceBehavior<,>),
                    typeof(ValidationBehavior<,>),
                    typeof(ExceptionBehavior<,>),
                    typeof(DomainEventDispatcherBehavior<,>)
                ];
            });

            // Регистрация FluentValidation
            services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly, includeInternalTypes: true);

            // Карты сортировки
            services.AddSingleton<ISortMap<Product>, ProductSortMap>();
            // services.AddSingleton<ISortMap<Order>,   OrderSortMap>();
            // services.AddSingleton<ISortMap<User>,    UserSortMap>();

            return services;
        }

    }
}
