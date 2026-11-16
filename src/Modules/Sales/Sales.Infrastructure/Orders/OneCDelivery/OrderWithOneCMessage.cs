using BuildingBlocks.Infrastructure.Outbox.Persistence;

namespace Sales.Infrastructure.Orders;

/// <summary>An order with its 1C outbox message; the message is null once retention removed it.</summary>
internal sealed class OrderWithOneCMessage
{
    public Order Order { get; init; } = null!;

    public OutboxMessage? Message { get; init; }
}
