using Catalog.Application.Features.Shared;
using Catalog.Application.Shared;

namespace Catalog.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IQuery<Result<ProductDto>>;
