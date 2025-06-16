namespace Application.Common.Abstractions.Common
{
    public interface IHasPagination
    {
        int PageNumber { get; }
        int PageSize { get; }
    }
}
