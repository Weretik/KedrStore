using BuildingBlocks.Application.Outbox;

namespace Sales.Application.Features.QuickOrders.Messages;

/// <summary>
/// A site quick order, stored in <c>sales."OutboxMessages"</c> and sent to the managers' Telegram chat as an Excel
/// file (platform spec 006, R-007). Plain values only: the outbox serializes it as JSON.
/// </summary>
public sealed record QuickOrderPlaced(
    Guid OrderId,
    string FirstName,
    string Phone,
    IReadOnlyList<QuickOrderLine> Lines,
    DateTimeOffset PlacedAtUtc) : IOutboxMessage
{
    /// <summary>The stored type name; keep it when the record is renamed.</summary>
    public const string TypeName = "sales.quick-order-placed";
}
