using BuildingBlocks.Application.Outbox;

namespace Sales.Application.Features.Orders.OneCDelivery.Messages;

/// <summary>
/// 1C did not get an order after every attempt: the managers get the order as an Excel file in Telegram
/// (platform spec 006, R-013).
/// </summary>
public sealed record OrderOneCDeliveryFailed(long OrderId, int Attempts, string? LastError, DateTimeOffset FailedAtUtc)
    : IOutboxMessage
{
    /// <summary>The stored type name; keep it when the record is renamed.</summary>
    public const string TypeName = "sales.order-one-c-delivery-failed";
}
