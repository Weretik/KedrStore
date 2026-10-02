using Catalog.Contracts.Products.GetMissingPhotos;

namespace Catalog.Application.Features.Products.GetMissingPhotos;

public sealed record GetMissingProductPhotosQuery(GetMissingProductPhotosRequest Request)
    : IQuery<Result<PagedResult<List<MissingProductPhotoListRowDto>>>>;
