# ADR 0051: Use IBusinessRule and BusinessRuleValidationException

## Date
2025-06-18

## Status
Accepted

## Context

In the domain layer, business rules are essential for ensuring the integrity and correctness of the domain model. To enforce these rules, the `IBusinessRule` interface is introduced, which defines a contract for implementing business rules. Additionally, the `BusinessRuleValidationException` class is used to handle violations of these rules, providing a structured approach to error management.

## Decision

We decided to use `IBusinessRule` and `BusinessRuleValidationException` in the domain layer to:

1. Define and enforce business rules in a consistent manner.
2. Provide a centralized mechanism for handling business rule violations.
3. Improve the readability and maintainability of domain logic.
4. Facilitate meaningful error responses by encapsulating rule violations in exceptions.

## Consequences

### Positive

1. Ensures the integrity and correctness of the domain model.
2. Centralized and consistent handling of business rule violations.
3. Improved readability and maintainability of domain logic.
4. Simplifies debugging and error tracing by encapsulating rule violations in exceptions.

### Negative

1. Adds complexity to the domain layer by introducing additional abstractions.
2. Requires developers to adhere to the `IBusinessRule` pattern.
3. May require additional effort to document and maintain business rules.

## Examples

### Positive Example

When a business rule is violated, the application throws a `BusinessRuleValidationException` with the broken rule:

```csharp
if (!businessRule.IsSatisfied())
{
    throw new BusinessRuleValidationException(businessRule);
}
```

This exception is caught and processed to provide meaningful feedback:

```csharp
catch (BusinessRuleValidationException ex)
{
    Console.WriteLine(ex.ToString());
}
```

### Negative Example

If developers bypass the `IBusinessRule` pattern and implement business rules directly in the domain logic, it can lead to inconsistent rule enforcement and poor maintainability:

```csharp
if (someCondition)
{
    throw new Exception("Business rule violated.");
}
```

This results in unstructured error handling and makes debugging more difficult.

## Implementation

The `IBusinessRule` interface defines a contract for implementing business rules, including a `Message` property and an `IsSatisfied` method. The `BusinessRuleValidationException` class encapsulates violations of these rules, providing a structured approach to error management.

## References

- ADR 0050: Use AppException for Error Handling in Application Layer
