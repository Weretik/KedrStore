namespace Domain.Common.ValueObject;

public abstract class EntityId : ValueObject
{
    public int Value { get; }

    protected EntityId(int value)
    {
        if (value <= 0)
            throw new Exception("ID не может быть меньше или равен нулю.");

        Value = value;
    }

    /*TODO
    / Реализовать DomainException и шаблон ошибки "ID не может быть меньше или равен нулю."

    */
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(EntityId id) => id.Value;
    /*

    public static bool operator ==(EntityId? left, EntityId? right)
        => Equals(left, right);

    public static bool operator !=(EntityId? left, EntityId? right)
        => !Equals(left, right);

    */
}
