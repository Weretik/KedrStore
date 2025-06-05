# Application Layer in KedrStore

## Overview

The Application layer contains the business logic of the application, defines contracts for interaction with external layers, and implements the CQRS (Command Query Responsibility Segregation) pattern. This layer connects the domain model to the outside world while adhering to the Dependency Inversion Principle (DIP).

## Key Principles

1. **Dependency Inversion (DIP)** – Application layer defines interfaces that are implemented in the Infrastructure layer
2. **Mediator Pattern** – CQRS is implemented using the MediatR library
3. **Loose Coupling** – Domain events are used to communicate between components
4. **Input Validation** – Handled using FluentValidation
5. **Data Mapping** – Performed with AutoMapper

## Components of the Application Layer

### 1. Commands and Queries (CQRS)

The main application logic is implemented through the CQRS pattern:

- **Commands (ICommand)** – for operations that modify data
- **Queries (IQuery)** – for data retrieval operations
- **Handlers** – contain the business logic to process commands and queries

### 2. Interfaces for Infrastructure

The Application layer defines interfaces implemented in the Infrastructure layer:

- **IApplicationDbContext** – access to the database
- **ICurrentUserService** – current user information
- **IEmailService** – email sending
- **IFileStorageService** – file storage
- **IBackgroundJobService** – background jobs
- **ICacheService** – data caching
- **IDateTimeProvider** – date/time access
- **IUnitOfWork** – Unit of Work pattern

### 3. Domain Event System

This layer includes support for domain events to ensure loose coupling:

- **IDomainEvent** – base interface for events
- **IDomainEventHandler** – event handler interface
- **IDomainEventDispatcher** – dispatcher interface

### 4. Support Components

- **Behaviors** – MediatR behaviors (logging, validation, event dispatch)
- **Exceptions** – application-level exceptions
- **Mapping** – AutoMapper profiles
- **Validation** – FluentValidation validators

## Folder Structure

```
src/Application/
│
├── Common/
│   ├── Abstractions/                # Interfaces and abstractions
│   │   ├── Common/                  # Generic abstractions
│   │   ├── Commands/                # Command abstractions
│   │   ├── Events/                  # Event abstractions
│   │   └── Queries/                 # Query abstractions
│   ├── Behaviors/                   # MediatR behaviors
│   ├── Constants/                   # Application constants
│   ├── Events/                      # Event implementations
│   ├── Exceptions/                  # Application-level exceptions
│   ├── Extensions/                  # Extension methods
│   ├── Mapping/                     # AutoMapper configuration
│   └── Validation/                  # FluentValidation logic
│
├── DependencyInjection/             # DI registration
├── Interfaces/                      # Interfaces for Infrastructure
└── UseCases/                        # Use case modules
```

## Interaction with Other Layers

### Interaction with Domain

The Application layer uses domain entities and rules defined in the Domain layer. It coordinates domain operations but does not contain domain logic itself.

### Interaction with Infrastructure

Interfaces defined in Application are implemented in Infrastructure, keeping the Application layer independent of implementation details.

### Interaction with Presentation

The Presentation layer (e.g., WebAPI, Blazor) uses Application commands and queries to perform business operations. It does not access Domain or Infrastructure directly.

## Example Execution Flow

1. User interacts with the UI
2. Presentation layer sends a command/query
3. MediatR routes the request to its handler
4. Behaviors (middleware) handle validation and logging
5. The handler executes business logic using domain entities
6. Domain events are raised if needed
7. Behaviors dispatch events after the main logic
8. The result is returned to the Presentation layer

## Patterns and Examples

### Example Command

```csharp
public class CreateProductCommand : ICommand<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IRepository<Product> _repository;

    public CreateProductCommandHandler(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product(command.Name, command.Price, command.Description);
        await _repository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}
```

### Example Query

```csharp
public class GetProductByIdQuery : IQuery<ProductDto>
{
    public int Id { get; set; }
}

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(query.Id, cancellationToken);
        if (product == null)
            throw new NotFoundException(nameof(Product), query.Id);

        return _mapper.Map<ProductDto>(product);
    }
}
```

## Extension Guidelines

1. Organize new use cases by business modules inside the `UseCases` folder
2. Each module should have `Commands` and `Queries` subfolders
3. Follow the “one class per file” principle
4. Each handler should do one thing only
5. Move shared logic to domain services or extension methods
6. Use domain events for side effects
7. Write unit tests for every command and query

The Application layer is a central part of KedrStore's architecture, coordinating between domain and external systems while enforcing clean architecture and inversion of control.
