using BuildingBlocks.Application.Outbox;
using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.Messages;

/// <summary>
/// Turns a stored site order into an order for 1C (spec sales/009, R-010). Stored with the site order under its
/// <see cref="QuickOrder.OneCOrderMessageId"/>, in the same save; the general outbox task runs it.
/// </summary>
public sealed record QuickOrderToOneC(Guid QuickOrderId) : IOutboxMessage
{
    /// <summary>The stored type name; keep it when the record is renamed.</summary>
    public const string TypeName = "sales.quick-order-to-1c";
}
