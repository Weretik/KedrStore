# ADR 0045: Use Throw for Error Handling

## Status
Accepted

## Context

In the application, error handling is a critical aspect of ensuring robustness and maintainability. To streamline error handling, the `Throw` class is introduced as a centralized utility for throwing exceptions based on `AppError`. This approach complements the existing error handling mechanism defined by `AppError`, `AppErrorCodes`, `AppErrorDescriptions`, `AppErrors`, and `AppException`.

## Decision

The `Throw` class will be used for throwing exceptions in a structured and consistent manner. It provides methods to throw `AppException` with detailed error information, including:

- Error code
- Error description
- Optional details
- Optional inner exception

This ensures that all exceptions are thrown in a standardized format, making them easier to handle and debug.

## Consequences

### Positive
- Centralized and consistent error handling.
- Improved readability and maintainability of error handling code.
- Enhanced debugging capabilities with detailed error information.

### Negative
- Developers must adhere to the `Throw` class for throwing exceptions, which may require refactoring existing code.

## Implementation

The `Throw` class is defined as follows:

```csharp
namespace Application.Common.Errors;

public static class Throw
{
    public static void Application(AppError error)
    {
        throw new AppException(error.Code, BuildMessage(error));
    }

    public static void Application(AppError error, string details)
    {
        throw new AppException(error.Code, $"{error.Description} | {details}");
    }

    public static void Application(AppError error, Exception inner)
    {
        throw new AppException(error.Code, BuildMessage(error), inner);
    }

    private static string BuildMessage(AppError error)
    {
        return error.Details is not null
            ? $"{error.Description} | {error.Details}"
            : error.Description;
    }
}
```

## Examples

### 1. Simple Throw
```csharp
Throw.Infrastructure(InfrastructureErrors.File.ReadError);
```

### 2. Throw with Details
```csharp
Throw.Infrastructure(InfrastructureErrors.File.NotFound.WithDetails($"Путь: {path}"));
```

### 3. Throw with Explicit Details
```csharp
Throw.Infrastructure(InfrastructureErrors.Seeder.DataMissing, "Отсутствует файл category.xml");
```

### 4. Throw with Inner Exception
```csharp
try
{
    File.ReadAllText("somefile.txt");
}
catch (Exception ex)
{
    Throw.Infrastructure(InfrastructureErrors.File.ReadError.WithDetails("Ошибка при чтении somefile.txt"), ex);
}
```

This ADR formalizes the use of the `Throw` class for error handling in the application.
