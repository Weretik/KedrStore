using IHasDomainEvents = Application.Common.Abstractions.Events.IHasDomainEvents;

namespace Application.Common.Behaviors
{
    public class DomainEventDispatcherBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly DbContext _dbContext;

        public DomainEventDispatcherBehavior(
            IDomainEventDispatcher domainEventDispatcher,
            DbContext dbContext)
        {
            _domainEventDispatcher = domainEventDispatcher;
            _dbContext = dbContext;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Обработка запроса
            var response = await next();

            // Получаем все сущности, которые могут иметь доменные события
            var entitiesWithEvents = _dbContext.ChangeTracker
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
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

            return response;
        }
    }
}
