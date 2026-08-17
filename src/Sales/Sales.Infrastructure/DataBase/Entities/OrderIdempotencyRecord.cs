namespace Sales.Infrastructure.DataBase.Entities;

internal sealed class OrderIdempotencyRecord
{
    public long Id { get; set; }
    public string Operation { get; set; } = null!;
    public string IdempotencyKey { get; set; } = null!;
    public string RequestHash { get; set; } = null!;
    public long OrderId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
}
