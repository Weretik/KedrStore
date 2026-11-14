using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;
namespace Catalog.Domain.Attributes;

/// <summary>ID of one value of a catalog attribute (for example «Антична бронза» of color).</summary>
public readonly record struct AttributeValueId
{
    public int Value { get; }

    private AttributeValueId(int value) => Value = value;

    public static AttributeValueId Create(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Attribute value ID must be positive.");

        return new AttributeValueId(value);
    }

    /// <summary>EF Core uses temporary keys before PostgreSQL assigns the identity value.</summary>
    public static AttributeValueId FromStorage(int value) => new(value);
}
