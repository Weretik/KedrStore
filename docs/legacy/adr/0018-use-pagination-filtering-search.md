# ADR 0018: Use Pagination, Filtering, and Search

## Date
2025-06-17

## Status
Accepted

## Context
Pagination, filtering, and search are essential features for modern e-commerce platforms. They allow users to efficiently navigate large datasets, refine results based on specific criteria, and locate items of interest. Implementing these features in the Kedr E-Commerce Platform ensures scalability, usability, and performance.

## Decision
We decided to implement pagination, filtering, and search in the project to:

1. Enhance user experience by providing efficient navigation and refined results.
2. Ensure scalability for large datasets by limiting the number of items returned per request.
3. Align with best practices for modern web applications.
4. Utilize reusable components such as `PagedResult`, `PagedQuery`, and `QueryableExtensions` for consistent implementation.

## Consequences
### Positive
1. Improves user experience by enabling efficient navigation and refined searches.
2. Ensures scalability for handling large datasets.
3. Promotes code reuse and consistency through shared components.
4. Simplifies integration with frontend frameworks and APIs.

### Negative
1. Adds complexity to the backend logic for handling pagination, filtering, and search.
2. Requires careful optimization to avoid performance bottlenecks with large datasets.
3. Developers must ensure proper validation and handling of user input for filtering and search.

## Example
Pagination, filtering, and search are implemented using the following components:

**PagedQuery.cs**:
```csharp
public abstract record PagedQuery<TResponse> : IQuery<TResponse>, IHasPagination
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}
```

**PagedResult.cs**:
```csharp
public class PagedResult<T> : IPagedList<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

**QueryableExtensions.cs**:
```csharp
public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}
```

These components ensure a consistent and reusable approach to implementing pagination, filtering, and search across the application.
