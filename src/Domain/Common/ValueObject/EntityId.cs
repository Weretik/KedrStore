namespace Domain.Common.ValueObject;

public abstract class EntityId(int value) : ValueObject
{
    public int Value { get; } = Guard.Against.NegativeOrZero(value, nameof(value));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
    public static explicit operator int(EntityId id) => id.Value;
}
