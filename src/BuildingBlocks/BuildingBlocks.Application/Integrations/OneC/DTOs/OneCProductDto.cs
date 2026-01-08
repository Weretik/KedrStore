namespace BuildingBlocks.Application.Integrations.OneC.DTOs;

public sealed record OneCProductDto(
    string Id,
    string Name,
    string CategoryPath,
    string Manufacturer,
    bool IsSale,
    bool IsNew,
    bool ExportToSite,
    int QuantityInPack);
