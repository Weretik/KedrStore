namespace Catalog.Application.Integrations.OneC.DTOs;

public sealed record OneCProductDto(
    int Id,
    string Name,
    string CategoryPath,
    string Manufacturer,
    bool IsSale,
    bool IsNew,
    bool ExportToSite,
    int QuantityInPack);
