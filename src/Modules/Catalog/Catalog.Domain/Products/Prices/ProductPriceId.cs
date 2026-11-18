namespace Catalog.Domain.Products.Prices;

/// <summary>Database identity of a product price row.</summary>
public readonly record struct ProductPriceId : IComparable<ProductPriceId>
{
    public int Value { get; }

    private ProductPriceId(int value) => Value = value;

    public static ProductPriceId Create(int value) => value > 0
        ? new ProductPriceId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static ProductPriceId FromStorage(int value) => new(value);

    public int CompareTo(ProductPriceId other) => Value.CompareTo(other.Value);
}
