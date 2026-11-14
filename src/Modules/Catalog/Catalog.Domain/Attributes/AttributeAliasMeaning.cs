using Catalog.Domain.Categories;

namespace Catalog.Domain.Attributes;

/// <summary>
/// One more thing a code means besides its own attribute (spec catalog/009): a composite 1C code such as <c>ZCG</c> may
/// carry a configuration and a colour at once. Products whose names contain the code get <see cref="ValueId"/> of
/// <see cref="AttributeId"/>. <see cref="CategoryId"/> <c>null</c>: for all products; otherwise only in that category,
/// where it belongs to the category's own decoding.
/// </summary>
public class AttributeAliasMeaning : IAggregateRoot
{
    public int Id { get; private set; }
    public AttributeAliasId AliasId { get; private set; }
    public ProductCategoryId? CategoryId { get; private set; }
    public AttributeId AttributeId { get; private set; }
    public AttributeValueId ValueId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private AttributeAliasMeaning() { }

    public static AttributeAliasMeaning Create(AttributeAliasId aliasId, ProductCategoryId? categoryId,
        AttributeId attributeId, AttributeValueId valueId, DateTimeOffset now)
        => new()
        {
            AliasId = aliasId, CategoryId = categoryId, AttributeId = attributeId, ValueId = valueId, UpdatedAt = now
        };

    /// <summary>Returns whether the value changed.</summary>
    public bool Change(AttributeValueId valueId, DateTimeOffset now)
    {
        if (ValueId == valueId)
            return false;

        ValueId = valueId;
        UpdatedAt = now;
        return true;
    }
}
