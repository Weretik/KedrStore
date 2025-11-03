using Application.Catalog.Shared;

namespace Application.Catalog.GetProducts;

public sealed record GetProductsQuery(
    ProductFilter Filter,
    ProductPagination Pagination,
    ProductSorter Sorter,
    PricingOptions PricingOptions)
    : IQuery<Result<GetProductsQueryResult<ProductDto>>>;
