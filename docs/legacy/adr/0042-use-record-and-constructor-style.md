# ADR 0042: Use Record and Constructor Style

## Date
2025-06-17

## Status
Accepted

## Context
Using `record` types and constructor-based initialization is a modern approach in C# that simplifies the definition of immutable data structures and ensures clean and concise code. In the Kedr E-Commerce Platform, this style is used for defining queries, commands, and handlers, such as `GetProductsQuery` and `GetProductsQueryHandler`. It aligns with best practices for modern C# development and improves readability and maintainability.

## Decision
We decided to use `record` types and constructor-based initialization in the project to:

1. Simplify the definition of immutable data structures.
2. Improve readability and maintainability of code.
3. Align with modern C# practices introduced in .NET 5 and later.
4. Ensure consistency in the implementation of queries, commands, and handlers.

## Consequences
### Positive
1. Simplifies the definition of immutable data structures.
2. Improves readability and maintainability of code.
3. Promotes consistency across the application.
4. Aligns with modern C# practices, ensuring compatibility with future updates.

### Negative
1. Requires developers to understand and correctly use `record` types and constructor-based initialization.
2. May introduce subtle bugs if immutability is not properly enforced.

## Example
This style is implemented as follows:

**GetProductsQuery.cs**:
```csharp
public sealed record GetProductsQuery : PagedQuery<AppResult<PagedResult<ProductDto>>>
{
    public string? SearchTerm { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? CategoryId { get; init; }
    public string? Manufacturer { get; init; }

    public GetProductsQuery() { }

    public GetProductsQuery(
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? categoryId = null,
        string? manufacturer = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        SortDirection sortDirection = default)
        : base(pageNumber, pageSize, sortBy, sortDirection)
    {
        SearchTerm = searchTerm;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        CategoryId = categoryId;
        Manufacturer = manufacturer;
    }
}
```

**GetProductsQueryHandler.cs**:
```csharp
public class GetProductsQueryHandler(
    IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetProductsQuery, AppResult<PagedResult<ProductDto>>>
{
    public async Task<AppResult<PagedResult<ProductDto>>> Handle(
        GetProductsQuery request, CancellationToken cancellationToken)
    {
        // Handler logic...
    }
}
```
