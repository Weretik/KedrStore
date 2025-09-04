namespace Application.Common.Notifications;

public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
