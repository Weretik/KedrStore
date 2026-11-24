namespace Sales.Domain.QuickOrders;

/// <summary>Where the delivery of a site order to the managers' Telegram chat stands (spec sales/009, R-005).</summary>
public enum QuickOrderTelegramStatus
{
    /// <summary>Not delivered yet; the outbox is still trying (a failed attempt leaves its error).</summary>
    Pending,

    /// <summary>Telegram has the order.</summary>
    Sent,

    /// <summary>The outbox gave up; a person may send it again.</summary>
    Failed
}
