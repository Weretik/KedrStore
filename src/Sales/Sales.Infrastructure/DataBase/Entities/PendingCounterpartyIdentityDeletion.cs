namespace Sales.Infrastructure.DataBase.Entities;

internal sealed class PendingCounterpartyIdentityDeletion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CounterpartyId { get; set; } = null!;
    public Guid IdentityUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastAttemptAtUtc { get; set; }
    public string? LastError { get; set; }
}
