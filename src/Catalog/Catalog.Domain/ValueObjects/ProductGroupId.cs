namespace Catalog.Domain.ValueObjects;

/// <summary>ID of a product variant group. Hundreds of rows, so an <c>int</c> identity.</summary>
public readonly record struct ProductGroupId
{
    public int Value { get; }

    private ProductGroupId(int value) => Value = value;

    public static ProductGroupId Create(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Product group ID must be positive.");

        return new ProductGroupId(value);
    }

    /// <summary>EF Core uses temporary keys before PostgreSQL assigns the identity value.</summary>
    public static ProductGroupId FromStorage(int value) => new(value);
}
