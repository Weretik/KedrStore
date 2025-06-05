using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Application.Common.Abstractions.Events;
using Application.Common.Abstractions.Common;
using Application.Common.Abstractions.Commands;
using Application.Common.Behaviors;
using Application.Common.Mapping;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection
{
    /// <summary>
    /// Методы расширения для регистрации сервисов приложения
    /// </summary>
    public static class ApplicationServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует сервисы слоя приложения
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <returns>Коллекция сервисов с добавленными сервисами приложения</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Регистрация MediatR и поведений
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                // Добавляем поведение для отправки доменных событий
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatcherBehavior<,>));
            });

            // Регистрация обработчиков доменных событий из текущей сборки
            services.AddDomainEventHandlers(Assembly.GetExecutingAssembly());

            // Регистрация AutoMapper с профилями из сборки приложения
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }

        /// <summary>
        /// Регистрирует все обработчики доменных событий из указанной сборки
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="assembly">Сборка, содержащая обработчики</param>
        /// <returns>Коллекция сервисов с добавленными обработчиками</returns>
        public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services, Assembly assembly)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                              i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var handlerInterfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

                foreach (var handlerInterface in handlerInterfaces)
                {
                    services.AddScoped(handlerInterface, handlerType);
                }
            }

            return services;
        }

        /// <summary>
        /// Регистрирует обработчик для конкретного типа события
        /// </summary>
        /// <typeparam name="TEvent">Тип доменного события</typeparam>
        /// <typeparam name="THandler">Тип обработчика</typeparam>
        /// <param name="services">Коллекция сервисов</param>
        /// <returns>Коллекция сервисов с добавленным обработчиком</returns>
        public static IServiceCollection AddDomainEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : IDomainEvent
            where THandler : class, IDomainEventHandler<TEvent>
        {
            services.AddScoped<IDomainEventHandler<TEvent>, THandler>();
            return services;
        }
    }
}
