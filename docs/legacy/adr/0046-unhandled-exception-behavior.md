# ADR 0046: Unhandled Exception Behavior

## Status
Accepted

## Context

Unhandled exceptions can occur during request processing, potentially destabilizing the application and leading to unlogged errors. To address this, a dedicated behavior is introduced to capture and handle such exceptions.

## Decision

We will use **UnhandledExceptionBehavior** to:

- Capture unhandled exceptions during request processing.
- Log critical errors to ensure visibility and traceability.
- Prevent application crashes by providing a fallback mechanism.

## Consequences

- **Pros**:
  - Ensures application stability during unexpected failures.
  - Provides centralized logging for unhandled exceptions.

- **Cons**:
  - Slight performance overhead due to exception handling.

## Implementation

This behavior will be registered in the application's dependency injection container and executed as part of the Mediator pipeline.

## References
- ADR 0045: Use Pipeline Behaviors for Request Handling
