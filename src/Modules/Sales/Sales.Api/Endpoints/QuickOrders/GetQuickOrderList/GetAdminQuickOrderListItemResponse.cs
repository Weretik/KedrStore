namespace Sales.Api.Endpoints.QuickOrders.GetQuickOrderList;

/// <param name="TelegramStatus"><c>sent</c>, <c>pending</c> or <c>failed</c>.</param>
/// <param name="TelegramLastError">Why the last attempt failed, while the order is not delivered.</param>
public sealed record GetAdminQuickOrderListItemResponse(
    Guid OrderId,
    DateTimeOffset PlacedAtUtc,
    string FirstName,
    string Phone,
    int LineCount,
    decimal Total,
    string TelegramStatus,
    string? TelegramLastError);
