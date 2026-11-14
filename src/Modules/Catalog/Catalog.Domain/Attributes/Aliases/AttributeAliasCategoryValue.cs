using Catalog.Domain.Categories;
using Catalog.Domain.Attributes.ValueObjects;
namespace Catalog.Domain.Attributes.Aliases;

/// <summary>
/// The owner's decoding of a code for one category only (spec catalog/008): the same 1C code may mean different things
/// in handles and in doors. It wins over the code's decoding for all products. <see cref="ValueId"/> <c>null</c> means
/// the code means nothing to filter by in this category.
/// </summary>
public class AttributeAliasCategoryValue : IAggregateRoot
{
    public AttributeAliasId AliasId { get; private set; }
    public ProductCategoryId CategoryId { get; private set; }
    public AttributeValueId? ValueId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private AttributeAliasCategoryValue() { }

    public static AttributeAliasCategoryValue Create(AttributeAliasId aliasId, ProductCategoryId categoryId,
        AttributeValueId? valueId, DateTimeOffset now)
        => new() { AliasId = aliasId, CategoryId = categoryId, ValueId = valueId, UpdatedAt = now };

    /// <summary>Returns whether the value changed.</summary>
    public bool Change(AttributeValueId? valueId, DateTimeOffset now)
    {
        if (ValueId == valueId)
            return false;

        ValueId = valueId;
        UpdatedAt = now;
        return true;
    }
}
