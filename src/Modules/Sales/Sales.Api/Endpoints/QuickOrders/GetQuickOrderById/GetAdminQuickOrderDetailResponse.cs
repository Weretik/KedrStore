namespace Sales.Api.Endpoints.QuickOrders.GetQuickOrderById;

/// <param name="TelegramStatus"><c>sent</c>, <c>pending</c> or <c>failed</c>.</param>
/// <param name="TelegramLastError">Why the last attempt failed, while the order is not delivered.</param>
public sealed record GetAdminQuickOrderDetailResponse(
    Guid OrderId,
    DateTimeOffset PlacedAtUtc,
    string FirstName,
    string Phone,
    decimal Total,
    string TelegramStatus,
    DateTimeOffset? TelegramSentAtUtc,
    DateTimeOffset? TelegramLastAttemptAtUtc,
    string? TelegramLastError,
    IReadOnlyList<GetAdminQuickOrderItemResponse> Items);
