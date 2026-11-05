namespace Catalog.Contracts.SiteModels;

/// <summary>A row of «Вітрина сайту»: an on-site product with its model, variant values, and problems.</summary>
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

public sealed record SiteShowcaseListDto(IReadOnlyList<SiteShowcaseItemDto> Items, int TotalCount, int Page, int PageSize);

/// <summary>
/// A final category for the «Моделі» sidebar: its models and what needs attention. <c>ProblemCount</c> counts its
/// accepted models with a problem and its on-site products outside accepted models.
/// </summary>
public sealed record SiteModelCategoryDto(
    int Id,
    string Name,
    string? ParentName,
    int OnSiteProducts,
    int AcceptedModels,
    int ProposedModels,
    int ProblemCount);
