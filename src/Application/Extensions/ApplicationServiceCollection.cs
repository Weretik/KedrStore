using IDomainEvent = Application.Common.Abstractions.Events.IDomainEvent;

namespace Application.Extensions
{
    public static class ApplicationServiceCollection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Регистрация MediatR и поведений
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

                // Добавляем Behaviors
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatcherBehavior<,>));
            });

            // Регистрация обработчиков событий
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Регистрация обработчиков доменных событий из текущей сборки
            services.AddDomainEventHandlers(Assembly.GetExecutingAssembly());

            // Регистрация AutoMapper с профилями из сборки приложения
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

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

