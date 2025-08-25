namespace Application.Common.Notifications;

/*
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent)
    : INotification
    where TDomainEvent : IDomainEvent;
*/
public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
