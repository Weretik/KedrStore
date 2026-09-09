namespace Sales.Application.Features.Orders.DeliveryFailure;

public interface IOrderDeliveryFailureExporter
{
    DeadLetterExcelFile Build(DeadLetterNotification notification);
}
