using Catalog.Application.Features.Products.GetById.DTOs;
using Catalog.Application.Integrations.OneC.DTOs;

namespace Catalog.Application.Features.Products.GetById;

public sealed record ProductBySlugQuery(GetProductBySlugRequest Request) : IQuery<Result<ProductBySlugDto>>;
