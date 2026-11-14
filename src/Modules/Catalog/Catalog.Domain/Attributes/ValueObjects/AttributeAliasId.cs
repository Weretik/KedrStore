namespace Catalog.Domain.Attributes.ValueObjects;

/// <summary>ID of one spelling of an attribute value in 1C product names.</summary>
public readonly record struct AttributeAliasId
{
    public int Value { get; }

    private AttributeAliasId(int value) => Value = value;

    public static AttributeAliasId Create(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Attribute alias ID must be positive.");

        return new AttributeAliasId(value);
    }

    /// <summary>EF Core uses temporary keys before PostgreSQL assigns the identity value.</summary>
    public static AttributeAliasId FromStorage(int value) => new(value);
}
