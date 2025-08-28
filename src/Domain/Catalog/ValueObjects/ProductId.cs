namespace Domain.Catalog.ValueObjects;

public readonly record struct ProductId(int Value)
{
    public override string ToString() => Value.ToString();
    public static implicit operator int(ProductId id) => id.Value;
    public static explicit operator ProductId(int value) => new(value);
}
