namespace Domain.Catalog.ValueObjects;

public readonly record struct ProductPriceId(int Value) : IEntityId<int>
{
public override string ToString() => Value.ToString();
public static implicit operator int(ProductPriceId id) => id.Value;
public static explicit operator ProductPriceId(int value) => new(value);
}
