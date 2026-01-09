// Licensed to KedrStore Development Team under MIT License.

namespace Catalog.Domain.ValueObjects;

[ValueObject<int>]
public readonly partial struct ProductPriceId
{
    private static Validation Validate(int value)
    {
        if (value <= 0) return Validation.Invalid("Id must be positive.");
        if (value > 1_000_000_000) return Validation.Invalid("Id is too large.");
        return Validation.Ok;
    }
}
