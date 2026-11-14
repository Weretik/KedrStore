namespace Catalog.Contracts.SiteModels.Public;

/// <summary>
/// Model cards of the public site (spec catalog/007, stage 5). With a category: its models and the models of its
/// subcategories; attribute filters only in a final category (M-09). Without a category: the search over all models.
/// </summary>
public sealed record GetPublicSiteModelsRequest
{
    /// <summary>Attribute filters <c>attribute-slug:value-slug[,value-slug]</c>: one attribute any, different attributes all.</summary>
    public string[]? Filter { get; init; }

    /// <summary>Words of a model name or of a product name, or a 1C code (M-12).</summary>
    public string? Search { get; init; }

    public bool? InStock { get; init; }
    public bool? IsSale { get; init; }
    public bool? IsNew { get; init; }
    public decimal? PriceFrom { get; init; }
    public decimal? PriceTo { get; init; }

    /// <summary><c>default</c> (in stock first, then name), <c>price_asc</c>, <c>price_desc</c>, <c>name_asc</c>, or <c>new</c>.</summary>
    public string? Sort { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 24;
}
