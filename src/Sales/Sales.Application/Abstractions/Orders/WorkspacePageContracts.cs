namespace Sales.Application.Abstractions.Orders;

public sealed record WorkspacePagedInfo(int PageNumber, int PageSize, long TotalPages, long TotalRecords);
public sealed record WorkspacePage<T>(WorkspacePagedInfo PagedInfo, IReadOnlyList<T> Value);
