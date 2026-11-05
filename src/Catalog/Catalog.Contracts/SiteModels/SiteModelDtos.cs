namespace Catalog.Contracts.SiteModels;

/// <summary>A model in the «Моделі» list (spec catalog/007). <c>Status</c>: <c>proposed</c> or <c>accepted</c>.</summary>
/// <param name="Axes">Names of the model's axes (saved, or offered for a proposed model).</param>
/// <param name="Problems">Problem codes of the model and of its on-site products (R-08); empty when all is well.</param>
public sealed record SiteModelListItemDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    string Slug,
    string Status,
    int ProductCount,
    int OnSiteCount,
    decimal? MinPrice,
    string Photo,
    IReadOnlyList<string> Axes,
    IReadOnlyList<string> Problems);

public sealed record SiteModelListDto(IReadOnlyList<SiteModelListItemDto> Items, int TotalCount, int Page, int PageSize);

/// <summary>An axis of a model. <c>IsSaved</c> is false for an axis only offered for a proposed model.</summary>
/// <param name="Display"><c>swatch</c> (color circles), <c>button</c> (size, side), or <c>checkbox</c>.</param>
public sealed record SiteModelAxisDto(int AttributeId, string Code, string Name, string Display, bool IsSaved);

/// <summary>The value of one axis on one product; <c>ValueId</c> is null when it has no confirmed value.</summary>
public sealed record SiteModelProductValueDto(int AttributeId, int? ValueId, string? Name, bool IsManual);

/// <summary>A product of a model with its axis values and problems (R-08).</summary>
public sealed record SiteModelProductDto(
    int ProductId,
    string Name,
    string Slug,
    string Photo,
    decimal? RetailPrice,
    bool InStock,
    bool ExportToSite,
    bool IsMain,
    IReadOnlyList<SiteModelProductValueDto> Values,
    IReadOnlyList<string> Problems);

/// <summary>Description and SEO texts of a model in one language (<c>uk</c> or <c>ru</c>).</summary>
public sealed record SiteModelTextDto(
    string Language,
    string ShortDescription,
    string Description,
    string SeoTitle,
    string SeoDescription);

/// <summary>An attribute every on-site product of the model shares (not an axis): a line of «Характеристики».</summary>
public sealed record SiteModelCharacteristicDto(int AttributeId, string Name, IReadOnlyList<string> Values);

/// <summary>A site model with its products, axes, texts, and problems (spec catalog/007).</summary>
public sealed record SiteModelDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    string Slug,
    string Status,
    DateTimeOffset? AcceptedAt,
    string? AcceptedBy,
    int? MainProductId,
    IReadOnlyList<SiteModelAxisDto> Axes,
    IReadOnlyList<SiteModelProductDto> Products,
    IReadOnlyList<SiteModelTextDto> Texts,
    IReadOnlyList<SiteModelCharacteristicDto> Characteristics,
    IReadOnlyList<string> Problems);

/// <summary>
/// Site health for «Огляд» (R-08). <c>ProductsWithoutModel</c> is the hard error: on-site products in no model at all
/// (products of a proposed model wait in <c>ProposedModels</c>). The others are warnings.
/// </summary>
public sealed record SiteModelHealthDto(
    int ProductsWithoutModel,
    int ProductsWithoutAttributes,
    int ProductsMissingAxisValue,
    int ProductsWithSameCombination,
    int ModelsWithoutAxes,
    int ModelsWithoutSeo,
    int ModelsWithoutPhoto,
    int ProposedModels);
