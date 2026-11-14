namespace Sales.Application.Abstractions.DeliveryFailure;

public interface IDeadLetterNotifier
{
    Task SendAsync(
        DeadLetterNotification notification,
        DeadLetterExcelFile attachment,
        CancellationToken cancellationToken = default);
}
