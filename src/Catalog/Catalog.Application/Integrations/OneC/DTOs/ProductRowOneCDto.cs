namespace Catalog.Application.Integrations.OneC.DTOs;

public sealed record ProductRowOneCDto(
    int Id,
    string ProductTypeIdOneC,
    string ProducSlug,
    string Name,
    int CategoryId,
    string Photo,
    string Scheme,
    decimal Stock,
    bool IsSale,
    bool IsNew,
    int QuantityInPack
);
