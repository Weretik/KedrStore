namespace Domain.Common.ValueObject;

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    protected virtual void CheckRules() { }

    protected ValueObject()
    {
        CheckRules(); // fail-fast
    }
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate(17, (current, next) => current * 23 + next);
    }


    public static bool operator ==(ValueObject? a, ValueObject? b) => Equals(a, b);
    public static bool operator !=(ValueObject? a, ValueObject? b) => !Equals(a, b);

    public static T WithChanges<T>(T obj, Action<T> updateFunction) where T : ValueObject, new()
    {
        var clone = new T();
        updateFunction(clone);
        return clone;
    }
}
