# MediatR Pipeline – KedrStore

## Overview

This document describes the MediatR pipeline setup in the KedrStore project. It explains the role of each `IPipelineBehavior<TRequest, TResponse>` and how behaviors are chained to control request validation, logging, error handling, and potential future features like caching or transactions.

---

## Why Use MediatR Behaviors?

MediatR behaviors allow KedrStore to:

- Apply cross-cutting concerns (logging, validation, exception handling) without duplicating code
- Maintain separation of concerns
- Keep UseCase Handlers clean and focused
- Extend the pipeline with new features per request type (e.g. audit, cache)

---

## Pipeline Order (Actual)

```text
┌──────────────────────────────────────────────┐
│ Request Enters (Blazor UI or API Controller) │
└───────────────┬──────────────────────────────┘
                ▼
┌──────────────────────────────────────────────┐
│ ValidationBehavior<TRequest, TResponse>      │
│ - Runs all FluentValidation validators       │
│ - Stops pipeline if validation fails         │
└───────────────┬──────────────────────────────┘
                ▼
┌──────────────────────────────────────────────┐
│ LoggingBehavior<TRequest, TResponse>         │
│ - Logs request name and execution time       │
└───────────────┬──────────────────────────────┘
                ▼
┌──────────────────────────────────────────────┐
│ ExceptionHandlingBehavior<TRequest, TResponse>│
│ - Logs unhandled exceptions                  │
│ - Returns AppResult.Failure(AppError)        │
└───────────────┬──────────────────────────────┘
                ▼
┌──────────────────────────────────────────────┐
│ Actual Handler (e.g. GetProductsQueryHandler)│
└──────────────────────────────────────────────┘
```

---

## Behaviors in Project

### 1. `ValidationBehavior<TRequest, TResponse>`
Location: `Application/Common/Behaviors/ValidationBehavior.cs`

- Uses `IValidator<TRequest>` from FluentValidation
- If validation errors exist, returns `AppResult.Failure(AppError.Validation(...))`
- Does not pass to the next behavior if invalid

### 2. `LoggingBehavior<TRequest, TResponse>`
Location: `Application/Common/Behaviors/LoggingBehavior.cs`

- Logs when request starts and ends
- Measures elapsed time
- Structured logging via Serilog

```csharp
_logger.LogInformation("Handling {Request} {@Request}", typeof(TRequest).Name, request);
_logger.LogInformation("Handled {Request} in {Elapsed}ms", typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
```

### 3. `ExceptionHandlingBehavior<TRequest, TResponse>`
Location: `Application/Common/Behaviors/ExceptionHandlingBehavior.cs`

- Catches any unhandled exception
- Logs via `ILoggingService`
- Wraps response in `AppResult.Failure(AppError.Unexpected(...))`

---

## Registration

Registered in `Program.cs` via:

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
```

> 📌 Order matters: Validation must come before Logging, which comes before ExceptionHandling.

---

## Extending the Pipeline (Ideas)

| Behavior              | Purpose                        | Status    |
|-----------------------|--------------------------------|-----------|
| CachingBehavior       | Return from cache for queries  | Optional  |
| TransactionBehavior   | Wrap command in DB transaction | Planned   |
| AuditBehavior         | Track user actions             | Planned   |
| RetryBehavior         | Retry transient failures       | Optional  |

---

## Summary

KedrStore's MediatR pipeline is clean, layered, and extensible. Validation, logging, and exception handling are implemented as `IPipelineBehavior<TRequest, TResponse>`, keeping business logic clean and focused. New behaviors can be added modularly for cross-cutting concerns.

