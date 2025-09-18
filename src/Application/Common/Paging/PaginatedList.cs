namespace Application.Common.Paging;

public sealed record PaginatedList<T>(IReadOnlyList<T> Items, int Total, PageRequest PageRequest)
{
    public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, PageRequest.PageSize));
    public bool HasPrev => PageRequest.CurrentPage > 1;
    public bool HasNext => PageRequest.CurrentPage < TotalPages;
}
