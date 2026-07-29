# ADR 0050: Use AppException for Error Handling in Application Layer

## Date
2025-06-17

## Status
Accepted

## Context

Error handling is a critical aspect of the application layer. To ensure consistent and meaningful error management, the `AppException` class is introduced. This class encapsulates error codes, descriptions, and optional technical messages, providing a structured approach to handling errors. Additionally, the application uses `AppErrors`, `AppError`, `AppErrorCodes`, and `AppErrorDescriptions` to define and manage error details consistently across different layers.

## Decision

We decided to use `AppException` in the application layer to:

1. Provide a centralized mechanism for handling application-specific errors.
2. Encapsulate error details, including codes, descriptions, and technical messages.
3. Enable consistent mapping of `AppError` instances to `AppException`.
4. Facilitate meaningful responses to the presentation layer by catching and processing `AppException`.
5. Leverage `AppErrors`, `AppErrorCodes`, and `AppErrorDescriptions` to standardize error definitions and descriptions across the application.

## Consequences

### Positive

1. Centralized and consistent error handling across the application layer.
2. Improved readability and maintainability of error-related code.
3. Enhanced debugging capabilities with structured error information.
4. Simplifies integration with the presentation layer by providing meaningful error responses.

### Negative

1. Requires developers to adhere to the `AppException` pattern.
2. Adds complexity to error handling logic.
3. May require additional effort to document and maintain error codes and descriptions.

## Examples

### Positive Example

When a validation error occurs, the application throws an `AppException` with a specific error code and description:

```csharp
throw new AppException(AppErrors.System.Validation("Invalid input data.").Code, AppErrors.System.Validation("Invalid input data.").Description);
```

This exception is caught in the application layer and mapped to a user-friendly response:

```csharp
catch (AppException ex)
{
    return new AppResult { Success = false, Error = ex.ToAppError() };
}
```

### Negative Example

If developers bypass the `AppErrors` and `AppException` pattern and throw generic exceptions, it can lead to inconsistent error handling and poor user experience:

```csharp
throw new Exception("An unexpected error occurred.");
```

This results in unstructured error responses and makes debugging more difficult.

## Implementation

The `AppException` class is defined with properties for error code, description, and optional technical messages. It includes a static `From` method to create exceptions from `AppError` instances. This approach ensures that errors are consistently represented and handled across the application layer.

## References

- ADR 0014: Result Pattern for Error Handling
- ADR 0039: Use UseCaseException
