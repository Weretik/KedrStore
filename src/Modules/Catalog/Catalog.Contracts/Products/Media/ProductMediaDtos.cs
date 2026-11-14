namespace Catalog.Contracts.Products.Media;

/// <summary>Replaces the whole media set of a product (R-31): photos in order and an optional scheme.</summary>
public sealed record SetProductMediaRequest
{
    public IReadOnlyList<long> PhotoIds { get; init; } = [];
    public long? SchemeId { get; init; }
}

public sealed record ProductMediaItemDto(long Id, string Url, string FileName);

public sealed record ProductMediaDto(int ProductId, IReadOnlyList<ProductMediaItemDto> Photos, ProductMediaItemDto? Scheme);
