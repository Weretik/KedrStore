# ADR 0005: Use MediatR for CQRS Implementation

## Status
Accepted

## Date
2025-06-04

## Context
KedrStore applies the **CQRS (Command Query Responsibility Segregation)** pattern to separate read and write concerns in the Application layer. This improves scalability, testing, and maintainability of business logic.

To implement CQRS effectively, a mediator-based dispatch mechanism is required to decouple senders from receivers of requests (commands/queries).

**MediatR** was chosen as the library to implement this pattern due to its:

- Lightweight and mature implementation of the Mediator pattern
- First-class support in .NET ecosystem
- Seamless integration with DI
- Support for pipeline behaviors (e.g., validation, logging, transaction boundaries)

## Decision
KedrStore will use **MediatR** for dispatching:
- Commands (e.g., `CreateOrderCommand`)
- Queries (e.g., `GetProductByIdQuery`)
- Notifications (e.g., `ProductCreatedNotification`, if needed)

Pipeline behaviors will be used to implement:
- Validation (`FluentValidationBehavior`)
- Logging (`LoggingBehavior`)
- Exception handling (`ExceptionHandlingBehavior`)
- Unit of Work or Transaction scope (`UnitOfWorkBehavior`, future)

## Consequences

### Positive
- Decoupled application logic
- Modular and testable handlers
- Cross-cutting concerns handled transparently
- Promotes SRP and separation between orchestration and business logic

### Negative
- Adds some abstraction overhead
- Increases learning curve for new developers unfamiliar with MediatR
