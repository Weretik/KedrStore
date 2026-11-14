using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Persistence.OrderDrafts;

internal sealed class OrderDraftIdempotencyRecord
{
    public long Id { get; set; }
    public string IdempotencyKey { get; set; } = null!;
    public string RequestHash { get; set; } = null!;
    public OrderDraftId DraftId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}