using IHasDomainEvents = Application.Common.Abstractions.Events.IHasDomainEvents;

namespace Application.Common.Behaviors
{
    public class DomainEventDispatcherBehavior<TRequest, TResponse>(
        IDomainEventDispatcher domainEventDispatcher,
        IDomainEventContext eventContext)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Обработка запроса
            var response = await next();

            // Получаем все сущности, которые могут иметь доменные события
            var domainEvents  = eventContext.GetDomainEntities()
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Отправляем события на обработку
            foreach (var domainEvent in domainEvents)
                await domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);

            // Очищаем события у сущностей
            eventContext.ClearDomainEvents();

            return response;
        }
    }
}
