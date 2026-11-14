namespace Catalog.Domain.Products;

[ValueObject<int>]
public readonly partial struct ProductId
{
    private static Validation Validate(int value)
    {
        if (value <= 0) return Validation.Invalid("Код має бути більшим за нуль.");
        if (value > 1_000_000_000) return Validation.Invalid("Код завеликий.");
        return Validation.Ok;
    }
}
