using Catalog.Application.Features.Products.GetById.DTOs;
using Catalog.Application.Integrations.OneC.DTOs;

namespace Catalog.Application.Features.Products.GetById;

public sealed record ProductBySlugQuery(string Slug) : IQuery<Result<ProductBySlugDto>>;
