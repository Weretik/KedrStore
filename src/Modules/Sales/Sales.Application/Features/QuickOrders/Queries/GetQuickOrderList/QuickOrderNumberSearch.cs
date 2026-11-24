using System.Globalization;

namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;

/// <summary>
/// The order number people see: the first 8 hex digits of the order id, shown on the site after sending, in the Telegram
/// message and in the Admin (spec sales/009, R-012). A search word that is such a number, or a whole id, names a range of ids.
/// </summary>
public static class QuickOrderNumberSearch
{
    public const int Length = 8;

    /// <summary>The ids a search word can name; null when the word is neither a number nor a whole id.</summary>
    public static (Guid From, Guid To)? RangeOf(string word)
    {
        var text = word.Trim().TrimStart('#', '№');
        if (Guid.TryParse(text, out var id))
            return (id, id);
        if (text.Length != Length || !uint.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out _))
            return null;

        var prefix = text.ToLowerInvariant();
        return (Guid.Parse($"{prefix}-0000-0000-0000-000000000000"), Guid.Parse($"{prefix}-ffff-ffff-ffff-ffffffffffff"));
    }

    /// <summary>The number shown for an order id, as on the site and in Telegram.</summary>
    public static string NumberOf(Guid orderId) =>
        orderId.ToString("N", CultureInfo.InvariantCulture)[..Length].ToUpperInvariant();
}
