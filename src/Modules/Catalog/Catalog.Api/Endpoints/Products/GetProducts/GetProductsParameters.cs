using Catalog.Contracts.Products.GetList;
namespace Catalog.Api.Endpoints.Products.GetProducts;

/// <summary>
/// The query of the public product list; a missing value takes the default of <see cref="GetProductsRequest"/>.
/// The language and category come from the route only, and price type rules are never taken from the client
/// (the controller listed <c>Lang</c>, <c>CategorySlug</c> and <c>PriceTypeRules</c> as query values it ignored or
/// could not bind).
/// </summary>
public sealed record GetProductsParameters
{
    public string? SearchTerm { get; init; }
    public int? CategoryId { get; init; }
    public bool? InStock { get; init; }
    public ProductAvailability? Availability { get; init; }
    public bool? IsSale { get; init; }
    public bool? IsNew { get; init; }
    public int? PriceTypeId { get; init; }
    public decimal? PriceFrom { get; init; }
    public decimal? PriceTo { get; init; }
    public ProductSort? Sort { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }

    public GetProductsRequest ToRequest(string lang, string? categorySlug)
    {
        var defaults = new GetProductsRequest();
        return defaults with
        {
            Lang = lang,
            SearchTerm = SearchTerm,
            CategorySlug = categorySlug,
            CategoryId = CategoryId,
            InStock = InStock ?? defaults.InStock,
            Availability = Availability,
            IsSale = IsSale,
            IsNew = IsNew,
            PriceTypeId = PriceTypeId,
            PriceFrom = PriceFrom,
            PriceTo = PriceTo,
            Sort = Sort ?? defaults.Sort,
            Page = Page ?? defaults.Page,
            PageSize = PageSize ?? defaults.PageSize
        };
    }
}
