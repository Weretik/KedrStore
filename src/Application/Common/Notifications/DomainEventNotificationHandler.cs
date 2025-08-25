namespace Application.Common.Notifications;

public sealed class DomainEventNotificationHandler
    : INotificationHandler<DomainEventNotification>
{
    public ValueTask Handle(DomainEventNotification notification, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
