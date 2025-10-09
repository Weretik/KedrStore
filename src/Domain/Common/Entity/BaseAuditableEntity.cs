namespace Domain.Common.Entity;

public abstract class BaseAuditableEntity<TId> : BaseEntity<TId>
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }

    public void MarkAsCreated(DateTime now) => CreatedAt = now;
    public void MarkAsUpdated(DateTime now) => UpdatedAt = now;

    public void MarkAsDeleted(DateTime now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        MarkAsUpdated(now);
    }
}
