namespace Catalog.Contracts.MobileSync;

/// <summary>
/// The whole sales catalog for the offline manager app (spec platform/004, R-001). Deleted products and categories
/// are left out. Amounts are UAH prices as the order workspace quotes them; the server quote stays the authority.
/// </summary>
/// <param name="Version">Hash of the content; the same value is sent as the <c>ETag</c>.</param>
/// <param name="GeneratedAt">When the server built this answer; not part of the version.</param>
/// <param name="RetailPriceTypeId">The price type used when a customer has no rule and no default.</param>
public sealed record CatalogSnapshotResponse(
    string Version,
    DateTimeOffset GeneratedAt,
    int RetailPriceTypeId,
    IReadOnlyList<CatalogSnapshotPriceType> PriceTypes,
    IReadOnlyList<CatalogSnapshotCategory> Categories,
    IReadOnlyList<CatalogSnapshotProduct> Products);

public sealed record CatalogSnapshotPriceType(int Id, string Name);

public sealed record CatalogSnapshotCategory(int Id, int? ParentId, string Name, int SortOrder);

/// <param name="Name">Ukrainian name, or the 1C name when there is no translation.</param>
/// <param name="InStock">The same rule as the sales catalog: hardware needs more than 2 in stock.</param>
/// <param name="Photos">Photo URLs in display order; the first is the main photo.</param>
/// <param name="Scheme">Scheme URL, or <c>null</c>.</param>
public sealed record CatalogSnapshotProduct(
    int Id,
    string Slug,
    string Name,
    int CategoryId,
    decimal Stock,
    bool InStock,
    int QuantityInPack,
    bool IsSale,
    bool IsNew,
    IReadOnlyList<string> Photos,
    string? Scheme,
    IReadOnlyList<CatalogSnapshotPrice> Prices);

public sealed record CatalogSnapshotPrice(int PriceTypeId, decimal Amount);
