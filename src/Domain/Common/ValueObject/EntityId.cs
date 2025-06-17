namespace Domain.Common.ValueObject;

public abstract class EntityId : ValueObject
{
    public int Value { get; }

    protected EntityId(int value)
    {
        if (value <= 0)
            RuleChecker.Check(
                new IdMustBePositiveRule(value)
            );

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
    public static implicit operator int(EntityId id) => id.Value;
}
