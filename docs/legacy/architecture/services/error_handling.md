# Error Handling – KedrStore

## Overview

This document describes the error handling architecture of KedrStore, based entirely on the current implementation in the project codebase. It follows Clean Architecture principles and applies separation of concerns for domain validation, infrastructure failures, and application-level orchestration.

---

## Layered Responsibility

```text
┌────────────────────────────────────────────┐
│  Presentation Layer (Blazor UI)           │
│  - ErrorBoundary                           │
│  - try/catch blocks                        │
│  - Handles AppResult<AppError> in UI       │
└──────────────────┬─────────────────────────┘
                   │
                   ▼
┌────────────────────────────────────────────┐
│  Application Layer                         │
│  - AppError.cs                             │
│  - AppResult<T>.cs                         │
│  - Behaviors: ExceptionHandlingBehavior    │
└──────────────────┬─────────────────────────┘
                   │
                   ▼
┌────────────────────────────────────────────┐
│  Domain Layer                              │
│  - IBusinessRule                           │
│  - RuleChecker                             │
└──────────────────┬─────────────────────────┘
                   │
                   ▼
┌────────────────────────────────────────────┐
│  Infrastructure Layer                      │
│  - InfrastructureException                 │
│  - try/catch external calls                │
└────────────────────────────────────────────┘
```

---

## Core Components

### `AppError`
Location: `Application/Common/Results/AppError.cs`

Defines typed error codes and static factory methods:

```csharp
public sealed record AppError(string Code, string Message)
{
    public string? Details { get; private init; }

    public static AppError NotFound(string entity, string? id = null) =>
        new("NotFound", $"{entity} not found{(id is not null ? $" (ID: {id})" : null)}");

    public static AppError Validation(string message) => new("Validation", message);
    public static AppError Unexpected(string? details = null) => new("Unexpected", "An unexpected error occurred")
        with { Details = details };
    // ... other static methods
}
```

### `AppResult<T>`
Location: `Application/Common/Results/AppResult.cs`

```csharp
public sealed class AppResult<T>
{
    public bool IsSuccess => Error is null;
    public bool IsFailure => !IsSuccess;
    public AppError? Error { get; init; }
    public T? Value { get; init; }

    public static AppResult<T> Success(T value) => new() { Value = value };
    public static AppResult<T> Failure(AppError error) => new() { Error = error };
}
```

Used instead of throwing exceptions inside use cases.

---

## Application-Level Behavior

### `ExceptionHandlingBehavior<TRequest, TResponse>`
Location: `Application/Common/Behaviors/ExceptionHandlingBehavior.cs`

Handles uncaught exceptions inside MediatR pipeline:

```csharp
public async Task<TResponse> Handle(...)
{
    try
    {
        return await next();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception for {Request}", typeof(TRequest).Name);
        return (TResponse)(object)AppResult.Failure<TResponse>(AppError.Unexpected());
    }
}
```

---

## Domain Layer Validation

### `IBusinessRule`
Location: `Domain/Common/Rules/IBusinessRule.cs`

```csharp
public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
}
```

### `RuleChecker`
Location: `Domain/Common/Rules/RuleChecker.cs`

```csharp
public static class RuleChecker
{
    public static void Check(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }
}
```

Domain validation is enforced via explicit rule objects, not by exceptions like `ArgumentException`.

---

## Infrastructure-Level Errors

### `InfrastructureException`
Location: `Infrastructure/Shared/Exceptions/InfrastructureException.cs`

Used to wrap external or low-level errors:

```csharp
public sealed class InfrastructureException(string code, string description, Exception? inner = null)
    : Exception(description, inner)
{
    public string Code { get; } = code;
    public string Description { get; } = description;
}
```

Infrastructure components log and rethrow wrapped exceptions if needed.

---

## UI Layer

- `ErrorBoundary` Blazor component is used per Razor UI component.
- Unhandled `AppResult.Failure(...)` can be logged or shown via UI wrappers.
- `CorrelationIdMiddleware` ensures that errors can be traced in logs.

---

## Logging Integration

- All error paths log through `ILoggingService`
- Behaviors log centrally in Application
- Infrastructure errors are logged when caught and rethrown
- Blazor UI may log through services or display `AppError.Message`

---

## Recommendations & Future Steps

| Area                        | Action                     |
|-----------------------------|-----------------------------|
| Global ErrorBoundary        | Wrap App.razor layout       |
| `AppErrorDisplay` component | Show user-facing errors     |
| Correlation ID in logs      | Already implemented         |
| API / JSON error format     | Future integration plan     |
| FluentValidation → UI map   | UI-friendly error output    |

---

## Summary

The KedrStore error handling strategy uses `AppResult<T>` + `AppError` in the Application layer, `IBusinessRule` in the Domain layer, and structured exception wrapping in Infrastructure. It cleanly separates technical, domain, and validation concerns and supports full observability via logging and correlation IDs.

