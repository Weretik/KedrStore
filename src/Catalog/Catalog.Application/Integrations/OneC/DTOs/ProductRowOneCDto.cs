namespace Catalog.Application.Integrations.OneC.DTOs;

/// <param name="CategoryId">Resolved category, or <c>null</c> when the 1C category name did not resolve (R-023).</param>
public sealed record ProductRowOneCDto(
    int Id,
    string ProductTypeIdOneC,
    string Name,
    int? CategoryId,
    string Photo,
    string Scheme,
    decimal Stock,
    bool IsSale,
    bool IsNew,
    bool ExportToSite,
    int QuantityInPack
);
