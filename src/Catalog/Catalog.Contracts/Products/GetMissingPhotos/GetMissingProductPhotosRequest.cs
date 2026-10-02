namespace Catalog.Contracts.Products.GetMissingPhotos;

public sealed record GetMissingProductPhotosRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
