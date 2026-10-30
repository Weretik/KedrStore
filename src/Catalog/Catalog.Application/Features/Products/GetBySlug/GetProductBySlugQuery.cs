using Catalog.Application.Features.Products.GetBySlug.DTOs;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Products.GetBySlug;

public sealed record GetProductBySlugQuery(GetProductBySlugRequest Request) : IQuery<Result<CatalogLookup<ProductBySlugDto>>>;
