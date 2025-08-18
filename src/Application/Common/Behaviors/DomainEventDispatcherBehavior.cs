namespace Application.Common.Behaviors
{
    public sealed class DomainEventDispatcherBehavior<TMessage, TResponse>(
        IDomainEventContext eventContext,
        IDomainEventDispatcher dispatcher,
        ILogger<DomainEventDispatcherBehavior<TMessage, TResponse>> logger)
        : IPipelineBehavior<TMessage, TResponse>
        where TMessage : IMessage
    {
        public async ValueTask<TResponse> Handle(
            TMessage message,
            MessageHandlerDelegate<TMessage, TResponse> next,
            CancellationToken ct)
        {
            var response = await next(message, ct);

            var entities = eventContext.GetDomainEntities();
            var events = entities.SelectMany(e => e.DomainEvents).ToList();

            if (events.Count == 0)
                return response;

            eventContext.ClearDomainEvents();
            DomainEventLog.Found(logger, typeof(TMessage).Name, events.Count);

            foreach (var @event in events)
            {
                var eventName = @event.GetType().Name;
                try
                {
                    DomainEventLog.Dispatching(logger, eventName);
                    await dispatcher.DispatchAsync(@event, ct);
                    DomainEventLog.Dispatched(logger, eventName);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    DomainEventLog.Failed(logger, eventName, ex);
                    throw;
                }
            }

            return response;
        }
    }
}
