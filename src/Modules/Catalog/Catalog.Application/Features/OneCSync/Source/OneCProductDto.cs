namespace Catalog.Application.Features.OneCSync.Source;

public sealed record OneCProductDto(
    int Id,
    string Name,
    string CategoryPath,
    string Manufacturer,
    bool IsSale,
    bool IsNew,
    bool ExportToSite,
    int QuantityInPack);
