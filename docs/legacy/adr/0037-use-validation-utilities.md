# ADR 0037: Use Validation Utilities

## Date
2025-06-17

## Status
Accepted

## Context
Validation is a critical aspect of application development, ensuring that data conforms to expected formats and rules before processing. The Kedr E-Commerce Platform uses custom validation utilities, such as `Ensure`, `ValidationUtils`, `ValidationResult`, and `ValidationError`, to provide a centralized and reusable approach to validation across the application layer.

## Decision
We decided to use custom validation utilities in the project to:

1. Centralize validation logic for better maintainability.
2. Provide reusable methods for common validation scenarios (e.g., null checks, range checks, email validation).
3. Ensure consistency in validation across the application layer.
4. Align with best practices for clean and maintainable architecture.

## Consequences
### Positive
1. Simplifies validation logic by providing reusable methods.
2. Improves maintainability by centralizing validation rules.
3. Ensures consistency in validation across different layers.
4. Reduces boilerplate code compared to manual validation.

### Negative
1. Adds complexity by introducing custom validation utilities.
2. Requires developers to understand and correctly use the validation utilities.

## Example
Validation utilities are implemented as follows:

**Ensure.cs**:
```csharp
public static class Ensure
{
    public static T NotNull<T>(T value, string parameterName)
    {
        if (value == null)
            throw new ArgumentNullException(parameterName);

        return value;
    }

    public static string NotNullOrWhiteSpace(string value, string parameterName)
    {
        NotNull(value, parameterName);

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", parameterName);

        return value;
    }
}
```

**ValidationUtils.cs**:
```csharp
public static class ValidationUtils
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            return Regex.IsMatch(email,
                "^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
```

**ValidationResult.cs**:
```csharp
public sealed record ValidationResult(IReadOnlyList<ValidationError> Errors)
{
    public bool IsValid => Errors.Count == 0;

    public static ValidationResult Success() => new([]);

    public static ValidationResult Failure(params ValidationError[] errors) => new(errors);
}
```
