using Application.Catalog.Shared;

namespace Application.Catalog.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IQuery<Result<ProductDto>>;
