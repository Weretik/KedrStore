using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;

public sealed record QuickOrderListItemDto(
    Guid OrderId,
    DateTimeOffset PlacedAtUtc,
    string FirstName,
    string Phone,
    int LineCount,
    decimal Total,
    QuickOrderTelegramStatus TelegramStatus,
    string? TelegramLastError);
