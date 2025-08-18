namespace Domain.Common.Entity;

public abstract class BaseEntity<TId> : IEntity<TId>, IHasDomainEvents
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; } = false;


    // --- Domain events ---
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    protected void Raise(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();

    // --- Audit helpers ---
    public void MarkAsCreated(DateTime now) => CreatedAt = now;
    public void MarkAsUpdated(DateTime now) => UpdatedAt = now;
    public void MarkAsDeleted(DateTime now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        MarkAsUpdated(now);
    }

    // --- Equality ---
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        if (IsTransient() || other.IsTransient()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public bool IsTransient()
        => EqualityComparer<TId>.Default.Equals(Id, default!);

    public override int GetHashCode()
        => HashCode.Combine(GetType(), Id);

    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right)
        => !(left == right);

}
