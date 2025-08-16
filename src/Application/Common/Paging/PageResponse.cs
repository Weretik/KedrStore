namespace Application.Common.Paging;

public sealed record PageResponse<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, PageSize));
    public bool HasPrev => Page > 1;
    public bool HasNext => Page < TotalPages;
}
