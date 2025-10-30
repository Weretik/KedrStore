namespace Application.Catalog.GetProducts;

public sealed record PaginationResult<T>(IReadOnlyList<T> Items, int Total, ProductPagination ProductPagination)
{
    public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, ProductPagination.PageSize));
    public bool HasPrev => ProductPagination.CurrentPage > 1;
    public bool HasNext => ProductPagination.CurrentPage < TotalPages;
}
