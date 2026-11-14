namespace Catalog.Contracts.SiteModels;

/// <summary>An on-site product inside a «Вітрина сайту» row, with its variant values and problems.</summary>
/// <param name="Model">Its model, or <c>null</c> when it is in none.</param>
/// <param name="Values">Confirmed values of the variant attributes (color, size, side), in attribute order.</param>
public sealed record SiteShowcaseItemDto(
    int ProductId,
    string Name,
    string Slug,
    string Photo,
    int CategoryId,
    string CategoryName,
    decimal? RetailPrice,
    bool InStock,
    SiteShowcaseModelDto? Model,
    IReadOnlyList<SiteShowcaseValueDto> Values,
    IReadOnlyList<string> Problems);

public sealed record SiteShowcaseModelDto(int Id, string Name, string Status);

public sealed record SiteShowcaseValueDto(int AttributeId, string AttributeName, string Value);

/// <summary>
/// A row of «Вітрина сайту»: a model with its on-site products, or one on-site product that is in no model.
/// <c>Key</c> is <c>m{modelId}</c> or <c>p{productId}</c>.
/// </summary>
/// <param name="Model">The model, or <c>null</c> for a product in no model.</param>
/// <param name="Problems">Problem codes of the model and of its products (R-08); empty when all is well.</param>
public sealed record SiteShowcaseRowDto(
    string Key,
    SiteShowcaseModelDto? Model,
    string Name,
    string Photo,
    int CategoryId,
    string CategoryName,
    decimal? MinPrice,
    bool InStock,
    IReadOnlyList<SiteShowcaseItemDto> Products,
    IReadOnlyList<string> Problems);

public sealed record SiteShowcaseListDto(IReadOnlyList<SiteShowcaseRowDto> Items, int TotalCount, int Page, int PageSize);

/// <summary>
/// A category shown on the site that has on-site products, for the category tree of «Вітрина сайту». Counts include
/// subcategories, except <c>FilterCount</c>.
/// </summary>
/// <param name="ProblemCount">Accepted models with a problem (R-08).</param>
/// <param name="LoneProducts">On-site products in no model (the hard error).</param>
/// <param name="FilterCount">Attributes the category itself offers as site filters (R-20).</param>
public sealed record SiteModelCategoryDto(
    int Id,
    string Name,
    int? ParentId,
    int OnSiteProducts,
    int AcceptedModels,
    int ProposedModels,
    int ProblemCount,
    int LoneProducts,
    int FilterCount);
