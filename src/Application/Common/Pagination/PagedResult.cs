namespace Application.Common.Pagination
{
    public class PagedResult<T> : IPagedList<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResult() { }

        public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items.ToList().AsReadOnly();
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public bool HasItems => Items.Any();

        public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 20) =>
            new ([], 0, pageNumber, pageSize);

        public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize) =>
            new PagedResult<T>(items, totalCount, pageNumber, pageSize);

        public void Deconstruct(out IReadOnlyList<T> items, out int totalCount, out int pageNumber, out int pageSize)
        {
            items = Items;
            totalCount = TotalCount;
            pageNumber = PageNumber;
            pageSize = PageSize;
        }
    }
}


