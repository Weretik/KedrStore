using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.UpdateValue;

/// <summary>Admin edits a value's names, order, and swatch; the dictionary import then leaves it.</summary>
public sealed record UpdateAttributeValueCommand(int AttributeId, int ValueId, UpdateAttributeValueRequest Request)
    : ICommand<Result<AdminCatalogAttributeDto>>;
