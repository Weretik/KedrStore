namespace BuildingBlocks.Domain.Identifiers;

/// <summary>Catalogue category (its 1C code); Sales customer price rules link to categories by this ID.</summary>
public readonly record struct ProductCategoryId : IComparable<ProductCategoryId>, IComparable
{
    public const int MaxValue = 1_000_000_000;

    public int Value { get; }

    private ProductCategoryId(int value) => Value = value;

    /// <summary>A 1C code: greater than zero and at most <see cref="MaxValue"/>.</summary>
    public static ProductCategoryId Create(int value) => value is > 0 and <= MaxValue
        ? new ProductCategoryId(value)
        : throw new ArgumentOutOfRangeException(nameof(value), value, "The code must be between 1 and " + MaxValue + ".");

    public static ProductCategoryId FromStorage(int value) => new(value);

    public int CompareTo(ProductCategoryId other) => Value.CompareTo(other.Value);

    int IComparable.CompareTo(object? obj) => obj is ProductCategoryId other ? CompareTo(other)
        : obj is null ? 1 : throw new ArgumentException("Expected " + nameof(ProductCategoryId) + ".", nameof(obj));

    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
