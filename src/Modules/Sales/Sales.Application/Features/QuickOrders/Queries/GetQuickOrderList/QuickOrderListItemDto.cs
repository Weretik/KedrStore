using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;

/// <param name="OneCOrderId">The order made from this site order for 1C (spec sales/009, R-015); null until it is made.</param>
/// <param name="OneCOrderNumber">That order's number, as the order history shows it.</param>
/// <param name="OneCDocumentNumber">The document number 1C gave that order; null until 1C accepts it.</param>
public sealed record QuickOrderListItemDto(
    Guid OrderId,
    DateTimeOffset PlacedAtUtc,
    string FirstName,
    string Phone,
    int LineCount,
    decimal Total,
    QuickOrderTelegramStatus TelegramStatus,
    string? TelegramLastError,
    long? OneCOrderId = null,
    string? OneCOrderNumber = null,
    string? OneCDocumentNumber = null);
