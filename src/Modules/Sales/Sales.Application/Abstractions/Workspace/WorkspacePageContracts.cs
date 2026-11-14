namespace Sales.Application.Abstractions.Workspace;

public sealed record WorkspacePagedInfo(int PageNumber, int PageSize, long TotalPages, long TotalRecords);
public sealed record WorkspacePage<T>(WorkspacePagedInfo PagedInfo, IReadOnlyList<T> Value);
