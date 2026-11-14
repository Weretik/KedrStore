using Catalog.Contracts.Products.Media;

namespace Catalog.Application.Features.ProductMedia.Queries.GetProductMedia;

public sealed record GetProductMediaQuery(int ProductId) : IQuery<Result<ProductMediaDto>>;
