using Catalog.Application.GetProducts;

namespace Catalog.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQueryResult<T>(
    IReadOnlyList<T> Items,
    int TotalItems,
    ProductPagination ProductPagination)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / Math.Max(1, ProductPagination.PageSize));
    public bool HasPrev => ProductPagination.CurrentPage > 1;
    public bool HasNext => ProductPagination.CurrentPage < TotalPages;
}
