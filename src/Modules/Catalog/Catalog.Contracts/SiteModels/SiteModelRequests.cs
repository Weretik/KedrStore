namespace Catalog.Contracts.SiteModels;

/// <summary>
/// The «Моделі» list. <c>CategoryId</c> takes the category with its subcategories; <c>Status</c>: <c>proposed</c> or <c>accepted</c>; <c>Problem</c>: a problem code (R-08);
/// <c>Search</c>: part of the model name, of a product name, or a 1C code.
/// </summary>
public sealed record GetSiteModelsRequest
{
    public int? CategoryId { get; init; }
    public string? Status { get; init; }
    public string? Problem { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>A model a person creates: accepted at once, its products pinned to it.</summary>
public sealed record CreateSiteModelRequest
{
    public int CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<int> ProductIds { get; init; } = [];
}

public sealed record RenameSiteModelRequest
{
    public string Name { get; init; } = string.Empty;
}

/// <summary>The final category the model moves to (spec catalog/007 R-22).</summary>
public sealed record ChangeSiteModelCategoryRequest
{
    public int CategoryId { get; init; }
}

public sealed record AcceptSiteModelsRequest
{
    public IReadOnlyList<int> Ids { get; init; } = [];
}

/// <summary>The model's axes, in switcher order.</summary>
public sealed record SetSiteModelAxesRequest
{
    public IReadOnlyList<int> AttributeIds { get; init; } = [];
}

public sealed record AddSiteModelProductsRequest
{
    public IReadOnlyList<int> ProductIds { get; init; } = [];
}

/// <summary>
/// All four texts of one language; a missing or blank text is stored empty. <c>Version</c> is the one the form read:
/// when the texts changed since, the save answers 409; without it the save is not checked.
/// </summary>
public sealed record SetSiteModelTextRequest
{
    public string? ShortDescription { get; init; }
    public string? Description { get; init; }
    public string? SeoTitle { get; init; }
    public string? SeoDescription { get; init; }
    public uint? Version { get; init; }
}

/// <summary>
/// «Вітрина сайту»: models with their on-site products. <c>CategoryId</c> takes the category with its subcategories;
/// <c>Status</c>: <c>proposed</c>, <c>accepted</c>, or <c>none</c> (products in no model);
/// <c>Problem</c>: a product problem code or <c>any</c>; <c>Search</c>: part of the model or product name, or a 1C code.
/// </summary>
public sealed record GetSiteShowcaseRequest
{
    public int? CategoryId { get; init; }
    public string? Status { get; init; }
    public string? Problem { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
