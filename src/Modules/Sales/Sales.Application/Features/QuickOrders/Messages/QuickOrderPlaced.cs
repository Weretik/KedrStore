using BuildingBlocks.Application.Outbox;
using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.Messages;

/// <summary>
/// Sends a site order to the managers' Telegram chat as an Excel file (platform spec 006, R-007; spec sales/009). The
/// order itself lives in <c>sales."QuickOrders"</c>; the message keeps a full copy so rows written before that table
/// existed are still delivered. Plain values only: the outbox serializes it as JSON.
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

    public static QuickOrderPlaced From(QuickOrder order) => new(
        order.Id.Value,
        order.FirstName,
        order.Phone,
        order.Items.Select(item => new QuickOrderLine(item.ProductId, item.Title, item.UnitPrice, item.Quantity)).ToList(),
        order.PlacedAtUtc);
}
