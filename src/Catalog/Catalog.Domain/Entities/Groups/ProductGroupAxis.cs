using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities.Groups;

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
