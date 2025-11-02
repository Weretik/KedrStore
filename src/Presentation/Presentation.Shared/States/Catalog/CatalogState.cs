using Application.Catalog.GetProducts;
using Application.Catalog.Shared;

namespace Presentation.Shared.States.Catalog;

[FeatureState]
public sealed record CatalogState(
    ProductFilter ProductsFilter,
    ProductSorter ProductsSorter,
    ProductPagination ProductsPagination,
    PricingOptions PricingOptions,
    bool IsLoading = false,
    string? Error = null,
    GetProductsQueryResult<ProductDto>? QueryResult = null)
{
    public CatalogState() : this(
        new ProductFilter(),
        new ProductSorter(),
        new ProductPagination(),
        new PricingOptions(),
        false,
        null,
        null) { }
}
