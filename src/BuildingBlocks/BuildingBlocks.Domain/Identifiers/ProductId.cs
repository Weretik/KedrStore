namespace BuildingBlocks.Domain.Identifiers;

/// <summary>Catalogue product (its 1C code); Sales order and draft lines link to products by this ID.</summary>
public readonly record struct ProductId : IComparable<ProductId>, IComparable
{
    public const int MaxValue = 1_000_000_000;

    public int Value { get; }

    private ProductId(int value) => Value = value;

    /// <summary>A 1C code: greater than zero and at most <see cref="MaxValue"/>.</summary>
    public static ProductId Create(int value) => value is > 0 and <= MaxValue
        ? new ProductId(value)
        : throw new ArgumentOutOfRangeException(nameof(value), value, "The code must be between 1 and " + MaxValue + ".");

    public static ProductId FromStorage(int value) => new(value);

    public int CompareTo(ProductId other) => Value.CompareTo(other.Value);

    int IComparable.CompareTo(object? obj) => obj is ProductId other ? CompareTo(other)
        : obj is null ? 1 : throw new ArgumentException("Expected " + nameof(ProductId) + ".", nameof(obj));

    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
