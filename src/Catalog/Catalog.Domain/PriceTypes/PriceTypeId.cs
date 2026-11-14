namespace Catalog.Domain.PriceTypes;

[ValueObject<int>]
public readonly partial struct PriceTypeId
{
    private static Validation Validate(int value)
    {
        if (value <= 0) return Validation.Invalid("Код має бути більшим за нуль.");
        if (value > 1_000_000) return Validation.Invalid("Код завеликий.");
        return Validation.Ok;
    }
}
