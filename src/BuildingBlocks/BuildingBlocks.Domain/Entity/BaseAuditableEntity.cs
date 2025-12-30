namespace BuildingBlocks.Domain.Entity;

public abstract class BaseAuditableEntity<TId> : BaseEntity<TId>
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }

    public void MarkAsCreated(DateTimeOffset now) => CreatedAt = now;
    public void MarkAsUpdated(DateTimeOffset now) => UpdatedAt = now;

    public void MarkAsDeleted(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        MarkAsUpdated(now);
    }
}
