using Catalog.Application.Shared;

namespace Catalog.Application.Features.Shared;

public sealed record ProductDto(
    int Id,
    string Name,
    string CategorySlug,
    string Photo,
    string Scheme,
    decimal Stock,
    bool IsSale,
    bool IsNew,
    int QuantityInPack,
    IReadOnlyList<ProductPriceDto> Prices
);
