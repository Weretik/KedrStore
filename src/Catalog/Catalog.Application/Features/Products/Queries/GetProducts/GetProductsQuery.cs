using Catalog.Application.Features.Shared;
using Catalog.Application.Shared;

namespace Catalog.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery(
    ProductFilter Filter,
    ProductPagination Pagination,
    ProductSorter Sorter,
    PricingOptions PricingOptions)
    : IQuery<Result<GetProductsQueryResult<ProductDto>>>;
