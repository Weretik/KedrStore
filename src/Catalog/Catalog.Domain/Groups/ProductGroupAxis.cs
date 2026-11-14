using Catalog.Domain.Categories.Content;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Domain.Groups;

/// <summary>
/// An attribute that tells the products of a model apart (color, size, side…): a switcher on the model page and a
/// filter of its category (spec catalog/007). <see cref="Position"/> orders the switchers.
/// </summary>
public class ProductGroupAxis : IAggregateRoot
{
    public ProductGroupId GroupId { get; private set; }
    public AttributeId AttributeId { get; private set; }
    public short Position { get; private set; }

    private ProductGroupAxis() { }

    public static ProductGroupAxis Create(ProductGroupId groupId, AttributeId attributeId, int position)
        => new() { GroupId = groupId, AttributeId = attributeId, Position = (short)position };

    public bool MoveTo(int position)
    {
        if (Position == position)
            return false;

        Position = (short)position;
        return true;
    }
}
