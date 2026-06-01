using Catalog.Application.Features.Products.GetBySlug.DTOs;

namespace Catalog.Application.Features.Products.GetBySlug;

public sealed record GetProductBySlugQuery(GetProductBySlugRequest Request) : IQuery<Result<ProductBySlugDto>>;
