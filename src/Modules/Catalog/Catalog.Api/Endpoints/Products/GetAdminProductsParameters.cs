using Catalog.Contracts.Products.GetAdminList;
using Catalog.Contracts.Products.GetList;

namespace Catalog.Api.Endpoints.Products;

/// <summary>The query of the admin product list; a missing value takes the default of <see cref="GetAdminProductsRequest"/>.</summary>
public sealed record GetAdminProductsParameters
{
    public string? SearchTerm { get; init; }
    public string? CategorySlug { get; init; }
    public int? CategoryId { get; init; }
    public bool? InStock { get; init; }
    public bool? IsSale { get; init; }
    public bool? IsNew { get; init; }
    public bool? ExportToSite { get; init; }
    public bool? Deleted { get; init; }
    public bool? WithoutPhoto { get; init; }
    public bool? ProblemsOnly { get; init; }
    public string? ProblemReason { get; init; }
    public decimal? PriceFrom { get; init; }
    public decimal? PriceTo { get; init; }
    public ProductSort? Sort { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }

    public GetAdminProductsRequest ToRequest()
    {
        var defaults = new GetAdminProductsRequest();
        return defaults with
        {
            SearchTerm = SearchTerm,
            CategorySlug = CategorySlug,
            CategoryId = CategoryId,
            InStock = InStock,
            IsSale = IsSale,
            IsNew = IsNew,
            ExportToSite = ExportToSite,
            Deleted = Deleted,
            WithoutPhoto = WithoutPhoto,
            ProblemsOnly = ProblemsOnly,
            ProblemReason = ProblemReason,
            PriceFrom = PriceFrom,
            PriceTo = PriceTo,
            Sort = Sort ?? defaults.Sort,
            Page = Page ?? defaults.Page,
            PageSize = PageSize ?? defaults.PageSize
        };
    }
}
