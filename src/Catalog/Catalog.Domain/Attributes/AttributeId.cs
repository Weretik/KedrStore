namespace Catalog.Domain.Attributes;

/// <summary>ID of a catalog attribute (color, size, side...). A small lookup table, so an <c>int</c> identity.</summary>
public readonly record struct AttributeId
{
    public int Value { get; }

    private AttributeId(int value) => Value = value;

    public static AttributeId Create(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Attribute ID must be positive.");

        return new AttributeId(value);
    }

    /// <summary>EF Core uses temporary keys before PostgreSQL assigns the identity value.</summary>
    public static AttributeId FromStorage(int value) => new(value);
}
