using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.CreateValue;

/// <summary>Admin adds a value to an attribute's list, for example a new decor.</summary>
public sealed record CreateAttributeValueCommand(int AttributeId, CreateAttributeValueRequest Request)
    : ICommand<Result<AdminCatalogAttributeDto>>;
