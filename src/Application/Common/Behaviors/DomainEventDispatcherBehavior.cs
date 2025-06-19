using IHasDomainEvents = Application.Common.Abstractions.Events.IHasDomainEvents;

namespace Application.Common.Behaviors
{
    public class DomainEventDispatcherBehavior<TRequest, TResponse>(
        IDomainEventDispatcher domainEventDispatcher,
        DbContext dbContext)
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
            var entitiesWithEvents = dbContext.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            // Собираем все доменные события
            var domainEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Очищаем события у сущностей
            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            // Отправляем события на обработку
            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

            return response;
        }
    }
}
