namespace Domain.Common.Entity;

public abstract class BaseEntity<TId> : IEntity<TId>, IHasDomainEvents
{
    public TId Id { get; protected set; } = default!;
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        if (IsTransient() || other.IsTransient()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public bool IsTransient() => EqualityComparer<TId>.Default.Equals(Id, default!);
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right) => !(left == right);

}
