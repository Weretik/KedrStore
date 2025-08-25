namespace Application.Common.Paging;

public sealed record PaginatedList<T>(
    IReadOnlyList<T> Items,
    int Total,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, PageSize));
    public bool HasPrev => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
