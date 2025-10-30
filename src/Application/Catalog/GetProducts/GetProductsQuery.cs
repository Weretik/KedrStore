using Application.Catalog.Shared;

namespace Application.Catalog.GetProducts;

public sealed record GetProductsQuery(ProductFilter Filter, ProductPagination ProductPagination, ProductSorter Sorter, int PriceTypeId)
    : IQuery<Result<PaginationResult<ProductDto>>>;
