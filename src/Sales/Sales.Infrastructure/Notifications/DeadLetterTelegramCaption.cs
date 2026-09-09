using Sales.Application.Features.Orders.DeliveryFailure;

namespace Sales.Infrastructure.Notifications;

public static class DeadLetterTelegramCaption
{
    public static string Build(DeadLetterNotification notification)
        => $"<b>Помилка доставки до 1С (DeadLetter)</b>\n" +
           $"ID замовлення: {Escape(notification.OrderId.ToString())}\n" +
           $"Номер замовлення: {Escape(notification.OrderNumber)}\n" +
           $"Контрагент: {Escape(notification.CounterpartyName)} ({Escape(notification.CounterpartyId)})\n" +
           $"Помилка: {Escape(notification.ErrorMessage ?? "Причину не вказано")}\n" +
           $"Спроб надсилання до 1С: {notification.AttemptCount}\n" +
           $"Час UTC: {notification.OccurredAtUtc:O}";

    private static string Escape(string value)
        => System.Net.WebUtility.HtmlEncode(value.Trim());
}
