namespace Sales.Api.Endpoints.QuickOrders.GetQuickOrderList;

/// <param name="TelegramStatus"><c>sent</c>, <c>pending</c> or <c>failed</c>.</param>
/// <param name="TelegramLastError">Why the last attempt failed, while the order is not delivered.</param>
/// <param name="OneCOrderId">The order made from this site order for 1C (spec sales/009, R-015); null until it is made.</param>
/// <param name="OneCOrderNumber">That order's number in the order history.</param>
/// <param name="OneCDocumentNumber">The document number 1C gave that order; null until 1C accepts it.</param>
public sealed record GetAdminQuickOrderListItemResponse(
    Guid OrderId,
    DateTimeOffset PlacedAtUtc,
    string FirstName,
    string Phone,
    int LineCount,
    decimal Total,
    string TelegramStatus,
    string? TelegramLastError,
    long? OneCOrderId,
    string? OneCOrderNumber,
    string? OneCDocumentNumber);
