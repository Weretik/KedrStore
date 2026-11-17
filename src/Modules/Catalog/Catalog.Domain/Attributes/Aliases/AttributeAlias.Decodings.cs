using Catalog.Domain.Attributes.ValueObjects;
using Catalog.Domain.Categories;

namespace Catalog.Domain.Attributes.Aliases;

/// <summary>
/// The owner's decodings of the code (specs catalog/008, catalog/009): its own value per category and its meanings for
/// other attributes, for all products or per category. Changed only through the code; reads query the tables directly.
/// </summary>
public partial class AttributeAlias
{
    private readonly List<AttributeAliasCategoryValue> _categoryValues = [];
    private readonly List<AttributeAliasMeaning> _meanings = [];

    /// <summary>Loaded only by the decision command.</summary>
    public IReadOnlyList<AttributeAliasCategoryValue> CategoryValues => _categoryValues;

    /// <summary>Loaded only by the decision command.</summary>
    public IReadOnlyList<AttributeAliasMeaning> Meanings => _meanings;

    /// <summary>The code's own decodings per category become exactly <paramref name="wanted"/>: one per category.</summary>
    public void ReplaceCategoryValues(IReadOnlyList<(ProductCategoryId CategoryId, AttributeValueId? ValueId)> wanted,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(wanted);
        _categoryValues.RemoveAll(item => wanted.All(want => want.CategoryId != item.CategoryId));
        foreach (var want in wanted.DistinctBy(want => want.CategoryId))
        {
            var existing = _categoryValues.FirstOrDefault(item => item.CategoryId == want.CategoryId);
            if (existing is null)
                _categoryValues.Add(AttributeAliasCategoryValue.Create(Id, want.CategoryId, want.ValueId, now));
            else
                existing.Change(want.ValueId, now);
        }
    }

    /// <summary>
    /// The code's other meanings become exactly <paramref name="wanted"/>: those for all products
    /// (<paramref name="forAllProducts"/>), or those of every category (a category left out loses its meanings).
    /// One value per (category, attribute).
    /// </summary>
    public void ReplaceMeanings(
        IReadOnlyList<(ProductCategoryId? CategoryId, AttributeId AttributeId, AttributeValueId ValueId)> wanted,
        bool forAllProducts, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(wanted);
        bool InScope(AttributeAliasMeaning item) => forAllProducts ? item.CategoryId is null : item.CategoryId is not null;
        _meanings.RemoveAll(item => InScope(item) &&
            wanted.All(row => row.CategoryId != item.CategoryId || row.AttributeId != item.AttributeId));
        foreach (var row in wanted.DistinctBy(row => (row.CategoryId, row.AttributeId)))
        {
            var existing = _meanings.FirstOrDefault(item => InScope(item) && item.CategoryId == row.CategoryId &&
                                                            item.AttributeId == row.AttributeId);
            if (existing is null)
                _meanings.Add(AttributeAliasMeaning.Create(Id, row.CategoryId, row.AttributeId, row.ValueId, now));
            else
                existing.Change(row.ValueId, now);
        }
    }
}
