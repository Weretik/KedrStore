namespace Sales.Application.Features.Orders.DeliveryFailure;

public interface IDeadLetterNotifier
{
    Task SendAsync(
        DeadLetterNotification notification,
        DeadLetterExcelFile attachment,
        CancellationToken cancellationToken = default);
}
