# ADR 0007: Use Result<T> Type Instead of Exceptions for Application Flow

## Status
Accepted

## Date
2025-06-04

## Context
In traditional .NET development, exceptions are often used to indicate both system failures and expected business validation errors. However, using exceptions for flow control leads to performance issues and unclear intent.

KedrStore prioritizes **predictable and safe error handling** aligned with CQRS and Clean Architecture principles. Business rules and validation failures should not throw exceptions but instead return structured results.

## Decision
We will use a **Result<T>** type (or equivalent such as `OneOf<TSuccess, TFailure>`) to represent:
- Success
- Failure with message or error code
- Optional payload (on success)

This approach will be used:
- In all Application layer handlers (commands/queries)
- In service methods that can fail logically (e.g. business rules)
- For returning operation outcomes without throwing

## Example

```csharp
public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken ct)
{
    var product = await _repository.GetByIdAsync(request.Id);
    if (product is null)
        return Result.Failure<ProductDto>("Product not found");

    return Result.Success(_mapper.Map<ProductDto>(product));
}
```

## Consequences

### Positive
- Eliminates misuse of exceptions for control flow
- Makes failures explicit and type-safe
- Improves testability and branching logic
- Helps enforce consistent error handling across features

### Negative
- Requires more code to handle branching explicitly
- Developers must follow discipline to avoid `throw` in expected failures
