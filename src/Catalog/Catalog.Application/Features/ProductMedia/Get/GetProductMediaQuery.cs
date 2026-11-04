using Catalog.Contracts.Products.Media;

namespace Catalog.Application.Features.ProductMedia.Get;

public sealed record GetProductMediaQuery(int ProductId) : IQuery<Result<ProductMediaDto>>;
