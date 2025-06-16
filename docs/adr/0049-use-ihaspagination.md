# ADR 0049: Use Pagination Interfaces

## Date
2025-06-17

## Status
Accepted

## Context
Pagination is a critical feature for managing large datasets in modern applications. In the Kedr E-Commerce Platform, interfaces such as `IHasPagination` and `IPagedList<T>` are used to standardize the implementation of pagination logic, ensuring consistency and maintainability across the application layer.

## Decision
We decided to use pagination interfaces in the project to:

1. Standardize the representation and handling of paginated data.
2. Simplify the integration of pagination logic with queries and commands.
3. Ensure consistency in handling paginated results across the application layer.
4. Align with best practices for clean and maintainable architecture.

## Consequences
### Positive
1. Improves consistency in the implementation of paginated results.
2. Simplifies debugging and testing of pagination logic.
3. Promotes alignment with best practices for handling pagination.
4. Enhances maintainability by providing standardized interfaces.

### Negative
1. Adds complexity by introducing additional abstraction layers.
2. Requires developers to understand and correctly implement pagination interfaces.

## Example
Pagination interfaces are implemented as follows:

**IHasPagination.cs**:
```csharp
namespace Application.Common.Abstractions.Pagination;

public interface IHasPagination
{
    int PageNumber { get; }
    int PageSize { get; }
}
```

**IPagedList.cs**:
```csharp
namespace Application.Common.Abstractions.Pagination;

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
```

**Usage in Application Layer**:
```csharp
public abstract record PagedQuery<TResponse> : IQuery<TResponse>, IHasPagination
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
```
