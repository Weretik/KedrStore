# ADR 0039: Use UseCaseException

## Date
2025-06-17

## Status
Accepted

## Context
`UseCaseException` is a custom exception used to represent errors specific to application use cases. It provides a clear and consistent way to handle business logic errors that occur during the execution of application scenarios. In the Kedr E-Commerce Platform, `UseCaseException` is used to encapsulate errors related to specific use cases, ensuring that business logic remains cohesive and focused.

## Decision
We decided to use `UseCaseException` in the project to:

1. Provide a clear and consistent way to represent use case-specific errors.
2. Simplify error handling by centralizing the logic for use case errors.
3. Align with best practices for clean and maintainable error handling.

## Consequences
### Positive
1. Improves consistency in error handling across the application.
2. Simplifies debugging by providing a clear exception type for use case errors.
3. Enhances maintainability by centralizing use case error logic.

### Negative
1. Adds complexity by introducing a custom exception type.
2. Requires developers to correctly use `UseCaseException` in appropriate scenarios.

## Example
`UseCaseException` is implemented as follows:

**UseCaseException.cs**:
```csharp
public class UseCaseException : Exception
{
    public UseCaseException() { }

    public UseCaseException(string message) : base(message) { }

    public UseCaseException(string message, Exception innerException) : base(message, innerException) { }
}
```

**Usage in Application Layer**:
```csharp
public class OrderService
{
    public void ValidateOrder(Order order)
    {
        if (order.Items.Count == 0)
        {
            throw new UseCaseException("Order must contain at least one item.");
        }
    }
}
```
