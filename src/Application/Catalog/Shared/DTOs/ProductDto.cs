namespace Application.Catalog.Shared;

public sealed record ProductDto(
    int Id,
    string Name,
    int CategoryId,
    int ProductTypeId,
    string Photo,
    string? Scheme,
    decimal Stock,
    IReadOnlyList<ProductPriceDto> Prices
);
