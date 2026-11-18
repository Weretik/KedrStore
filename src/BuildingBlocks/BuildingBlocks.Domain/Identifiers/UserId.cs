namespace BuildingBlocks.Domain.Identifiers;

/// <summary>Identity of a signed-in account (ASP.NET Identity user); other modules link to users by this ID.</summary>
public readonly record struct UserId : IComparable<UserId>
{
    public Guid Value { get; }

    private UserId(Guid value) => Value = value;

    public static UserId Create(Guid value) => value != Guid.Empty
        ? new UserId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static UserId FromStorage(Guid value) => new(value);

    public int CompareTo(UserId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}
