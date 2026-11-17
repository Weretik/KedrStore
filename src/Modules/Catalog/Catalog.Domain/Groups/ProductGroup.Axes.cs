using Catalog.Domain.Attributes.ValueObjects;
using Catalog.Domain.Groups.Members;

namespace Catalog.Domain.Groups;

/// <summary>The model's axes (spec catalog/007 R-05): each attribute once, ordered by its place in the list.</summary>
public partial class ProductGroup
{
    private readonly List<ProductGroupAxis> _axes = [];

    /// <summary>Loaded only by the commands that change the axes.</summary>
    public IReadOnlyList<ProductGroupAxis> Axes => _axes;

    /// <summary>
    /// Replaces the axes with <paramref name="attributeIds"/> in that order: kept axes move, missing ones are removed,
    /// new ones are added. Returns whether anything changed.
    /// </summary>
    public bool SetAxes(IReadOnlyList<AttributeId> attributeIds)
    {
        ArgumentNullException.ThrowIfNull(attributeIds);
        var wanted = attributeIds.Distinct().ToList();
        var removed = _axes.RemoveAll(axis => !wanted.Contains(axis.AttributeId)) > 0;
        var changed = removed;
        for (var position = 0; position < wanted.Count; position++)
        {
            var existing = _axes.FirstOrDefault(axis => axis.AttributeId == wanted[position]);
            if (existing is null)
            {
                _axes.Add(ProductGroupAxis.Create(Id, wanted[position], position));
                changed = true;
            }
            else
            {
                changed |= existing.MoveTo(position);
            }
        }

        return changed;
    }
}
