namespace Catalog.Application.Integrations.OneC.DTOs;

/// <summary>A product row from 1C. Photos and schemes are not 1C data: they are linked from the media library (R-32).</summary>
/// <param name="CategoryId">Resolved category, or <c>null</c> when the 1C category name did not resolve (R-023).</param>
public sealed record ProductRowOneCDto(
    int Id,
    string ProductTypeIdOneC,
    string Name,
    int? CategoryId,
    decimal Stock,
    bool IsSale,
    bool IsNew,
    bool ExportToSite,
    int QuantityInPack
);
