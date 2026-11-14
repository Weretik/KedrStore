using Catalog.Domain.Categories;
using Catalog.Domain.Attributes.ValueObjects;

namespace Catalog.Domain.Attributes;

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

    /// <summary>A person sets the filter in «Фільтри категорій»; the dictionary import no longer changes it.</summary>
    public static CategoryAttribute CreateManual(ProductCategoryId categoryId, AttributeId attributeId,
        bool isFilterable, int sortOrder, DateTimeOffset now)
    {
        var filter = Create(categoryId, attributeId, isFilterable, sortOrder, now);
        filter.IsEditedManually = true;
        return filter;
    }

    /// <summary>A person changes the filter. Returns whether anything changed.</summary>
    public bool EditManually(bool isFilterable, int sortOrder, DateTimeOffset now)
    {
        if (IsEditedManually && IsFilterable == isFilterable && SortOrder == sortOrder)
            return false;

        IsFilterable = isFilterable;
        SortOrder = sortOrder;
        IsEditedManually = true;
        UpdatedAt = now;
        return true;
    }
}
