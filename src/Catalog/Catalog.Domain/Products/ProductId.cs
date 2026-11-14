using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;
namespace Catalog.Domain.Products;

[ValueObject<int>]
public readonly partial struct ProductId
{
    private static Validation Validate(int value)
    {
        if (value <= 0) return Validation.Invalid("Id must be positive.");
        if (value > 1_000_000_000) return Validation.Invalid("Id is too large.");
        return Validation.Ok;
    }
}
