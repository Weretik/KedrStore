using Catalog.Application.Integrations.OneC.DTOs;

namespace Catalog.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IQuery<Result<ProductDto>>;
