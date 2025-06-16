namespace Application.Common.Abstractions.Pagination
{
    public interface IHasPagination
    {
        int PageNumber { get; }
        int PageSize { get; }
    }
}
