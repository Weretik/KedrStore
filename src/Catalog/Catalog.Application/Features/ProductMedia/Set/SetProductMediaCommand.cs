using Catalog.Contracts.Products.Media;

namespace Catalog.Application.Features.ProductMedia.Set;

public sealed record SetProductMediaCommand(int ProductId, SetProductMediaRequest Request)
    : ICommand<Result<ProductMediaDto>>;
