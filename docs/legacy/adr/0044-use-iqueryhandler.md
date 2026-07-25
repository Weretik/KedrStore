# ADR 0044: Use CQRS Interfaces

## Date
2025-06-17

## Status
Accepted

## Context
CQRS (Command Query Responsibility Segregation) is a design pattern that separates the handling of queries and commands. In the Kedr E-Commerce Platform, interfaces such as `IQueryHandler`, `IQuery`, `ICommandHandler`, and `ICommand` are used to standardize the implementation of queries and commands, ensuring consistency and maintainability across the application.

## Decision
We decided to use CQRS interfaces in the project to:

1. Standardize the implementation of queries and commands.
2. Simplify the integration of MediatR with the application layer.
3. Ensure consistency in handling queries and commands.
4. Align with best practices for clean and maintainable architecture.

## Consequences
### Positive
1. Improves consistency in the implementation of queries and commands.
2. Simplifies debugging and testing of query and command logic.
3. Promotes alignment with the CQRS pattern and MediatR.
4. Enhances maintainability by providing standardized interfaces.

### Negative
1. Adds complexity by introducing additional abstraction layers.
2. Requires developers to understand and correctly implement CQRS interfaces.

## Example
CQRS interfaces are implemented as follows:

**IQueryHandler.cs**:
```csharp
namespace Application.Common.Abstractions.UseCase;

public interface IQueryHandler<TQuery, TResult>
    : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>;
```

**IQuery.cs**:
```csharp
namespace Application.Common.Abstractions.UseCase;

public interface IQuery<TResult>
    : IRequest<TResult>, IUseCase { }
```

**ICommandHandler.cs**:
```csharp
namespace Application.Common.Abstractions.UseCase;

public interface ICommandHandler<TCommand, TResult>
    : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>;
```

**ICommand.cs**:
```csharp
namespace Application.Common.Abstractions.UseCase;

public interface ICommand<TResult> : IRequest<TResult>, IUseCase { }
public interface ICommand : ICommand<Unit> { }
```
