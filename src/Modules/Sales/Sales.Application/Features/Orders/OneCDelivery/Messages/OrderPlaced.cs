using BuildingBlocks.Application.Outbox;

namespace Sales.Application.Features.Orders.OneCDelivery.Messages;

/// <summary>
/// An admin or mobile order to deliver to 1C (platform spec 006, R-010), stored in <c>sales."OutboxMessages"</c> under
/// the order's <c>OneCMessageId</c> in the transaction that creates the order.
/// </summary>
public sealed record OrderPlaced(long OrderId) : IOutboxMessage
{
    /// <summary>The stored type name; keep it when the record is renamed.</summary>
    public const string TypeName = "sales.order-placed";
}
