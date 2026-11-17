using BuildingBlocks.Domain.Abstractions;

namespace BuildingBlocks.Domain.Entity;

public abstract class BaseEntity<TId> : IEntity<TId>
{
    public TId Id { get; protected set; } = default!;
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
