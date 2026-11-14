using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Persistence.Orders.Idempotency;

internal sealed class OrderIdempotencyRecord
{
    public long Id { get; set; }
    public string Operation { get; set; } = null!;
    public string IdempotencyKey { get; set; } = null!;
    public string RequestHash { get; set; } = null!;
    public OrderId OrderId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
}
