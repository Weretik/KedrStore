namespace Application.Common.Abstractions.Common
{
    public interface IPagedList<out T>
    {
        int PageNumber { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        IReadOnlyList<T> Items { get; }
    }
}
