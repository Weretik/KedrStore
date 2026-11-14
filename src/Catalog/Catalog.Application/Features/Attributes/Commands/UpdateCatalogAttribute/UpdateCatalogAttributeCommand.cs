using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Commands.UpdateCatalogAttribute;

/// <summary>Admin edits an attribute's names, filter flag, and order; the dictionary import then leaves it.</summary>
public sealed record UpdateCatalogAttributeCommand(int AttributeId, UpdateCatalogAttributeRequest Request)
    : ICommand<Result<AdminCatalogAttributeDto>>;
