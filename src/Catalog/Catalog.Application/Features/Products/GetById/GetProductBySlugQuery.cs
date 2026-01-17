using Catalog.Application.Features.Products.GetById.DTOs;

namespace Catalog.Application.Features.Products.GetById;

public sealed record GetProductBySlugQuery(GetProductBySlugRequest Request) : IQuery<Result<ProductBySlugDto>>;
