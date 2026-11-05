using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Get;

/// <summary>The «Властивості» panel of a product (spec catalog/006, phase 3).</summary>
public sealed record GetProductAttributesQuery(int ProductId) : IQuery<Result<AdminProductAttributesDto>>;
