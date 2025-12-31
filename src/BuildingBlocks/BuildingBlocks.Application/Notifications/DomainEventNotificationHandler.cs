namespace BuildingBlocks.Application.Notifications;

public sealed class DomainEventNotificationHandler
    : INotificationHandler<DomainEventNotification>
{
    public ValueTask Handle(DomainEventNotification notification, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
