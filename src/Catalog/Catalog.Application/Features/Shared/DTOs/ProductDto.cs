using Catalog.Application.Shared;

namespace Catalog.Application.Features.Shared;

public sealed record ProductDto(
    int Id,
    string Name,
    int CategoryId,
    int ProductTypeId,
    string Photo,
    string? Scheme,
    decimal Stock,
    int QuantityInPack,
    IReadOnlyList<ProductPriceDto> Prices
);
