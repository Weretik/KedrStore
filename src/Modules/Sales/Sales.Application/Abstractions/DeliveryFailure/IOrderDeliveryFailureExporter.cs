namespace Sales.Application.Abstractions.DeliveryFailure;

public interface IOrderDeliveryFailureExporter
{
    DeadLetterExcelFile Build(DeadLetterNotification notification);
}
