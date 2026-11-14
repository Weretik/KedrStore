using System.Net;
using Sales.Application.Features.QuickOrders.Messages;

namespace Sales.Infrastructure.QuickOrders;

/// <summary>
/// The Telegram caption of a quick order. Customer text is HTML-escaped; the order id is shown so the managers notice
/// a repeated send (platform spec 006, R-005).
/// </summary>
public static class QuickOrderTelegramCaption
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static string Build(QuickOrderPlaced order)
    {
        var placedAt = TimeZoneInfo.ConvertTime(order.PlacedAtUtc, Kyiv);
        string[] parts =
        [
            "<b>Нове замовлення</b>",
            $"🆔 <b>{order.OrderId.ToString("N")[..8].ToUpperInvariant()}</b> <code>{order.OrderId}</code>",
            $"👨‍💼 {WebUtility.HtmlEncode(order.FirstName)}",
            $"📲 <code>{WebUtility.HtmlEncode(order.Phone)}</code>",
            $"📅 {placedAt:dd.MM.yyyy HH:mm}",
        ];

        return string.Join(Environment.NewLine, parts);
    }
}
