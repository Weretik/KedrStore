namespace Domain.Common.ValueObject;

public abstract class EntityId : ValueObject
{
    public int Value { get; }

    protected EntityId(int value)
    {
        RuleChecker.Check (new IdMustBePositiveRule(value));
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
    public static explicit operator int(EntityId id) => id.Value;
}
