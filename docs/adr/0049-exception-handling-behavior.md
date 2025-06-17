# ADR 0048: Validation Behavior

## Status
Accepted

## Context

Validating incoming requests is crucial to ensure data integrity and correctness before processing. A dedicated behavior is introduced to perform validation using FluentValidation.

## Decision
# ADR 0049: Exception Handling Behavior
We will use **ValidationBehavior** to:

- Validate incoming requests using FluentValidation.
- Ensure data integrity and correctness before processing.

## Consequences

- **Pros**:
  - Prevents processing of invalid data.
  - Centralized validation logic.

- **Cons**:
  - Slight performance overhead due to validation.

## Implementation

This behavior will be registered in the application's dependency injection container and executed as part of the Mediator pipeline.

## References
- ADR 0022: Use FluentValidation
- ADR 0045: Use Pipeline Behaviors for Request Handling

## Status
Accepted

## Context

Application-specific exceptions (`AppException`) need to be handled gracefully and mapped to appropriate response types. A dedicated behavior is introduced to manage this process.

## Decision

We will use **ExceptionHandlingBehavior** to:

- Handle application-specific exceptions (`AppException`).
- Map exceptions to appropriate response types, such as `AppResult` or `AppResult<T>`.

## Consequences

- **Pros**:
  - Provides consistent error handling across the application.
  - Improves user experience by returning meaningful responses.

- **Cons**:
  - Slight increase in complexity due to exception mapping.

## Implementation

This behavior will be registered in the application's dependency injection container and executed as part of the Mediator pipeline.

## References
- ADR 0039: Use UseCaseException
- ADR 0045: Use Pipeline Behaviors for Request Handling
