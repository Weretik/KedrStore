using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;
namespace Catalog.Domain.PriceTypes;

[ValueObject<int>]
public readonly partial struct PriceTypeId
{
    private static Validation Validate(int value)
    {
        if (value <= 0) return Validation.Invalid("Id must be positive.");
        if (value > 1_000_000) return Validation.Invalid("Id is too large.");
        return Validation.Ok;
    }
}
