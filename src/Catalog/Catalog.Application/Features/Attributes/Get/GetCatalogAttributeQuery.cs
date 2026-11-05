using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Get;

/// <summary>One attribute with its values and the codes 1C names use for them.</summary>
public sealed record GetCatalogAttributeQuery(int AttributeId) : IQuery<Result<AdminCatalogAttributeDto>>;
