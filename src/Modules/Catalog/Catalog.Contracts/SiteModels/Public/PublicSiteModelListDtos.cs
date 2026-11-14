namespace Catalog.Contracts.SiteModels.Public;

/// <summary>
/// One page of model cards. <c>Filters</c> are the attribute filters of a final category with model counts (empty for
/// a parent category and for the search). <c>ExactMatch</c> is set when the search is the 1C code of an on-site product.
/// </summary>
public sealed record PublicSiteModelListDto(
    string? CategorySlug,
    bool IsFinalCategory,
    int Page,
    int PageSize,
    int TotalPages,
    int TotalRecords,
    IReadOnlyList<PublicSiteModelCardDto> Items,
    IReadOnlyList<PublicSiteModelFilterDto> Filters,
    PublicSiteModelAddressDto? ExactMatch);

/// <summary>
/// A model card. Price, stock, sale and new count only the products that match the chosen filters.
/// <c>IsAccepted</c> is false for an on-site product shown as a model of its own (M-04). <c>ProductId</c> is the 1C code
/// of the only on-site product of a one-product model, so the site can add it to the cart from the list.
/// </summary>
public sealed record PublicSiteModelCardDto(
    string Slug,
    string Name,
    string CategorySlug,
    string Photo,
    decimal? PriceFrom,
    bool InStock,
    bool IsSale,
    bool IsNew,
    int ProductCount,
    bool IsAccepted,
    IReadOnlyList<PublicSiteModelSwatchDto> Colors,
    int? ProductId);

/// <summary>A color of a value: <c>Hex</c> <c>#RRGGBB</c> or a texture picture; both empty means a text chip.</summary>
public sealed record PublicSiteModelSwatchDto(string Slug, string Name, string? Hex, string? TextureUrl);

/// <param name="Display"><c>swatch</c> (colors), <c>button</c> (variant attributes), or <c>checkbox</c>.</param>
public sealed record PublicSiteModelFilterDto(
    string Code,
    string Slug,
    string Name,
    string Display,
    IReadOnlyList<PublicSiteModelFilterValueDto> Values);

/// <summary>
/// A filter value with the number of models the list shows when it is added to the other attributes' choices; values
/// without models are left out unless selected.
/// </summary>
public sealed record PublicSiteModelFilterValueDto(
    string Slug,
    string Name,
    string? Hex,
    string? TextureUrl,
    int Count,
    bool Selected);

/// <summary>
/// Where a product lives on the site: its model and the axis values that choose it (axis slug → value key). The site
/// builds <c>/catalog/{CategorySlug}/{ModelSlug}?{axis}={value}</c> from it (M-11).
/// </summary>
public sealed record PublicSiteModelAddressDto(
    string ModelSlug,
    string CategorySlug,
    IReadOnlyDictionary<string, string> Selection);
