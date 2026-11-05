using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities.Attributes;

/// <summary>An attribute shown as a filter in one category, in the given order.</summary>
public class CategoryAttribute : IAggregateRoot
{
    public ProductCategoryId CategoryId { get; private set; }
    public AttributeId AttributeId { get; private set; }
    public bool IsFilterable { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsEditedManually { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private CategoryAttribute() { }

    public static CategoryAttribute Create(ProductCategoryId categoryId, AttributeId attributeId, bool isFilterable,
        int sortOrder, DateTimeOffset now)
        => new()
        {
            CategoryId = categoryId,
            AttributeId = attributeId,
            IsFilterable = isFilterable,
            SortOrder = sortOrder,
            UpdatedAt = now
        };
}
