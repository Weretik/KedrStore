using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderById;

/// <param name="OneCOrderId">See <see cref="GetQuickOrderList.QuickOrderListItemDto"/>.</param>
public sealed record QuickOrderDetailDto(
    Guid OrderId,
    DateTimeOffset PlacedAtUtc,
    string FirstName,
    string Phone,
    decimal Total,
    QuickOrderTelegramStatus TelegramStatus,
    DateTimeOffset? TelegramSentAtUtc,
    DateTimeOffset? TelegramLastAttemptAtUtc,
    string? TelegramLastError,
    IReadOnlyList<QuickOrderItemDto> Items,
    string? Comment = null,
    long? OneCOrderId = null,
    string? OneCOrderNumber = null,
    string? OneCDocumentNumber = null);
