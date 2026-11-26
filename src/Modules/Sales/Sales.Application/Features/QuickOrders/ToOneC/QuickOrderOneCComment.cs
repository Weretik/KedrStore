using Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;
using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.ToOneC;

/// <summary>
/// The comment of the 1C order made from a site order (spec sales/009, R-010): the order number the buyer saw, the
/// buyer's name and phone, and when it was placed, Kyiv time.
/// </summary>
public static class QuickOrderOneCComment
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static string Build(QuickOrder order)
    {
        var placedAt = TimeZoneInfo.ConvertTime(order.PlacedAtUtc, Kyiv);
        return $"Сайт № {QuickOrderNumberSearch.NumberOf(order.Id.Value)}, {order.FirstName}, {order.Phone}, {placedAt:dd.MM.yyyy HH:mm}";
    }
}
