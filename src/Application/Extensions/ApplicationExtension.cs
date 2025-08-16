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
                    typeof(LoggingBehavior<,>),
                    typeof(ExceptionHandlingBehavior<,>),
                    typeof(ValidationBehavior<,>),
                    typeof(PerformanceBehavior<,>),
                    typeof(DomainEventDispatcherBehavior<,>)
                ];
            });

            // Регистрация обработчиков событий
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Регистрация обработчиков доменных событий из текущей сборки
            services.AddDomainEventHandlers(Assembly.GetExecutingAssembly());

            // Регистрация FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
        public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services, Assembly assembly)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var handlerInterfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

                foreach (var handlerInterface in handlerInterfaces)
                {
                    services.AddScoped(handlerInterface, handlerType);
                }
            }

            return services;
        }
        public static IServiceCollection AddDomainEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : IDomainEvent
            where THandler : class, IDomainEventHandler<TEvent>
        {
            services.AddScoped<IDomainEventHandler<TEvent>, THandler>();
            return services;
        }
    }
}
