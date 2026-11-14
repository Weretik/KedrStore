using BuildingBlocks.Domain.Abstractions.Events;

namespace BuildingBlocks.Application.Notifications;

public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
