namespace Application.Common.Pagination;

public sealed record PagedMetadata(
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);
