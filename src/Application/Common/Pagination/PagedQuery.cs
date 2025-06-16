namespace Application.Common.Pagination
{
    public abstract record PagedQuery<TResponse> : IQuery<TResponse>, IHasPagination
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string? SortBy { get; init; }
        public SortDirection SortDirection { get; init; } = SortDirection.Asc;

        public PagedQuery() { }

        public PagedQuery(int pageNumber, int pageSize, string? sortBy, SortDirection sortDirection)
        {
            PageNumber = pageNumber <= 0 ? 1 : pageNumber;
            PageSize = pageSize <= 0 ? 20 : pageSize;
            SortBy = sortBy;
            SortDirection = sortDirection;
        }

        public void Deconstruct(out int pageNumber, out int pageSize)
        {
            pageNumber = PageNumber;
            pageSize = PageSize;
        }
    }
}
