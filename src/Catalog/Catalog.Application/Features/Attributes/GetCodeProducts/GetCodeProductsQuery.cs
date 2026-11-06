using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.GetCodeProducts;

/// <summary>Where a code occurs: on-site products whose names contain it, by category (spec catalog/008).</summary>
public sealed record GetCodeProductsQuery(int AliasId) : IQuery<Result<AdminCodeProductsDto>>;
