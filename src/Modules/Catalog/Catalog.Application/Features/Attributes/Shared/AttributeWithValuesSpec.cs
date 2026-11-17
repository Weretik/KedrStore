using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes.ValueObjects;

namespace Catalog.Application.Features.Attributes.Shared;

/// <summary>An attribute with its values, for the commands that add or change values through the attribute.</summary>
public sealed class AttributeWithValuesSpec : SingleResultSpecification<CatalogAttribute>
{
    public AttributeWithValuesSpec(AttributeId attributeId)
        => Query.Where(attribute => attribute.Id == attributeId).Include(attribute => attribute.Values);
}
