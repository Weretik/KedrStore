namespace Catalog.Contracts.SiteModels.Public;

/// <summary>
/// The model page (spec catalog/007, stage 5): the model's axes with the values its on-site products have, every
/// on-site product with its axis values, and the texts of the requested language. The site hides value combinations
/// no product has and offers in-stock neighbours of a product out of stock (M-10).
/// </summary>
public sealed record PublicSiteModelDto(
    string Slug,
    string Name,
    bool IsAccepted,
    PublicSiteModelCategoryDto Category,
    IReadOnlyList<PublicSiteModelCategoryDto> Breadcrumbs,
    PublicSiteModelTextDto Texts,
    string? MainProductSlug,
    IReadOnlyList<PublicSiteModelAxisDto> Axes,
    IReadOnlyList<PublicSiteModelProductDto> Products,
    IReadOnlyList<PublicSiteModelCharacteristicDto> Characteristics);

public sealed record PublicSiteModelCategoryDto(int Id, string Slug, string Name);

/// <summary>Texts of the language; an empty Russian text falls back to the Ukrainian one. Empty when not written.</summary>
public sealed record PublicSiteModelTextDto(
    string ShortDescription,
    string Description,
    string SeoTitle,
    string SeoDescription);

/// <param name="Display"><c>swatch</c> (colors) or <c>button</c>.</param>
public sealed record PublicSiteModelAxisDto(
    string Code,
    string Slug,
    string Name,
    string Display,
    IReadOnlyList<PublicSiteModelAxisValueDto> Values);

/// <summary>
/// An axis value. <c>Key</c> is the value slug (several values of one product join with <c>_</c>, a number is its
/// text); it is the query value of the model address.
/// </summary>
public sealed record PublicSiteModelAxisValueDto(string Key, string Name, string? Hex, string? TextureUrl);

/// <summary>An on-site product of the model; <c>Selection</c> maps an axis slug to the product's value key.</summary>
public sealed record PublicSiteModelProductDto(
    int Id,
    string Name,
    string Slug,
    string Photo,
    decimal? Price,
    bool InStock,
    bool IsSale,
    bool IsNew,
    bool IsMain,
    IReadOnlyDictionary<string, string> Selection);

/// <summary>An attribute outside the axes that every on-site product of the model has with the same values.</summary>
public sealed record PublicSiteModelCharacteristicDto(string Code, string Name, IReadOnlyList<string> Values);
