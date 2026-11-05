using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.GetList;

/// <summary>The attributes of the «Властивості» page in order.</summary>
public sealed record GetCatalogAttributesQuery : IQuery<Result<List<AdminCatalogAttributeListItemDto>>>;
