using Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;
using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.ToOneC;

/// <summary>
/// The comment of the 1C order made from a site order (spec sales/009, R-010): the order number the buyer saw, the
/// buyer's name and phone, when it was placed (Kyiv time) and the buyer's comment when there is one.
/// </summary>
public static class QuickOrderOneCComment
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static string Build(QuickOrder order)
    {
        var placedAt = TimeZoneInfo.ConvertTime(order.PlacedAtUtc, Kyiv);
        var text = $"Сайт № {QuickOrderNumberSearch.NumberOf(order.Id.Value)}, {order.FirstName}, {order.Phone}, {placedAt:dd.MM.yyyy HH:mm}";
        return order.Comment is null ? text : $"{text}. Коментар покупця: {order.Comment}";
    }
}
