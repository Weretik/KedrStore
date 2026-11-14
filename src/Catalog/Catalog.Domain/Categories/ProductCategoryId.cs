using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;
namespace Catalog.Domain.Categories;

[ValueObject<int>]
public readonly partial struct ProductCategoryId
{
    private static Validation Validate(int value)
    {
        if (value <= 0) return Validation.Invalid("Id must be positive.");
        if (value > 1_000_000_000) return Validation.Invalid("Id is too large.");
        return Validation.Ok;
    }
}
