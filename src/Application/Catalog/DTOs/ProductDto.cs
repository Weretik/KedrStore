namespace Application.Catalog.DTOs;

public sealed record ProductDto(
    int Id,
    string Name,
    int CategoryId,
    int ProductTypeId,
    string Photo,
    decimal Stock,
    IReadOnlyList<ProductPriceDto> Prices
);
