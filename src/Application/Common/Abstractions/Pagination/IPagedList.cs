namespace Application.Common.Abstractions.Pagination
{
    public interface IPagedList<out T>
    {
        int PageNumber { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        bool HasItems { get; }
        IReadOnlyList<T> Items { get; }
    }
}
