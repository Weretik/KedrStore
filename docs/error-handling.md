# Error Handling Strategy – KedrStore

## Goals

- Ensure consistent and predictable error handling across all layers
- Avoid leaking technical details to the UI or API consumers
- Separate infrastructure-level exceptions from business validation
- Make the system resilient and observable

---

## General Principles

1. **Centralized error handling** in Presentation (Blazor) and Middleware
2. **Business rule validation** is not considered exceptional — handled via result objects
3. **Only truly unexpected failures** throw exceptions
4. **All exceptions are logged**, but user sees safe messages
5. **Structured errors** in logs and optionally for API responses

---

## Error Sources and Strategies

| Layer             | Strategy                                                                 |
|-------------------|--------------------------------------------------------------------------|
| Domain            | Throw domain-specific exceptions (e.g. `ProductNotFoundException`)       |
| Application       | Use `Result<T>` pattern or `OneOf<TSuccess, TError>` to avoid exceptions |
| Infrastructure    | Catch external failures, rethrow as internal exceptions (e.g. `EmailSendFailureException`) |
| Presentation (UI) | Catch with `ErrorBoundary`, show user-friendly message                   |

---

## Tools and Patterns

- `Result<T>` type or a custom `OperationResult` type to represent success/failure
- `FluentValidation` for input validation
- Global `try/catch` and error logging in middleware
- `ErrorBoundary` component in Blazor for UI fault zones
- `ILogger` (Serilog) used for structured logging of all exceptions

---

## Planned Components

- ✅ Custom exceptions in Domain (e.g. `InvalidOrderStateException`)
- ✅ FluentValidation-based validators in Application
- ✅ Exception-to-response mapper (if API layer added)
- ✅ `ErrorBoundary` setup in Blazor App (stage-04+)
- ✅ Middleware-level `UseExceptionHandler()` in Program.cs

---

## Example: Application Layer Handler

```csharp
public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
{
    var product = await _repository.GetByIdAsync(request.Id);
    if (product is null)
        return Result.Failure<ProductDto>("Product not found");

    return Result.Success(_mapper.Map<ProductDto>(product));
}
```

---

## Logging Strategy

- All exceptions are logged via Serilog with `context + stack trace`
- No technical messages are exposed to end users
- Logs are enriched with correlation IDs (planned)
- Alerts (e.g., via email or Telegram) can be configured for critical errors (future)

---

## Future Enhancements

- Mapping domain exceptions to UI-level messages
- Unified `Result<T>` model across layers
- Blazor global fault boundary
- API-specific exception filters (if added)