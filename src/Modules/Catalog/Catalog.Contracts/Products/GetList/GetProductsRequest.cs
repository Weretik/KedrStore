namespace Catalog.Contracts.Products.GetList;

public sealed record GetProductsRequest
{
    public string Lang { get; init; } = "uk";
    public string? SearchTerm { get; init; }
    public string? CategorySlug { get; init; }
    public int? CategoryId { get; init; }

    public bool? InStock { get; init; } = true;
    /// <summary>When supplied, takes precedence over <see cref="InStock"/>.</summary>
    public ProductAvailability? Availability { get; init; }
    public bool? IsSale { get; init; }
    public bool? IsNew { get; init; }

    public int? PriceTypeId { get; init; }
    public IReadOnlyCollection<CategoryPriceTypeRule> PriceTypeRules { get; init; } = [];
    public decimal? PriceFrom { get; init; }
    public decimal? PriceTo { get; init; }

    public ProductSort Sort { get; init; } = ProductSort.IdAsc;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public enum ProductAvailability
{
    InStock,
    All
}
