# CQRS Pattern in KedrStore

## Overview

The KedrStore project uses the CQRS (Command Query Responsibility Segregation) pattern to separate read and write operations. This approach provides the following benefits:

- Clear separation of responsibilities between system components
- Simplified domain model for specific use cases
- Ability to optimize read and write operations independently
- Improved testability and maintainability

## Core Components

### Commands

Commands represent an intent to change the system state. In KedrStore, they implement the `ICommand<TResult>` or `ICommand` interface for commands without a return value.

Example command:
```csharp
public class CreateProductCommand : ICommand<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}
```

### Queries

Queries are used to retrieve data without changing the system state. They implement the `IQuery<TResult>` interface.

Example query:
```csharp
public class GetProductByIdQuery : IQuery<ProductDto>
{
    public int Id { get; set; }
}
```

### Handlers

Each command or query has a corresponding handler that contains the business logic:

- `ICommandHandler<TCommand, TResult>` – for command processing
- `IQueryHandler<TQuery, TResult>` – for query processing

Example command handler:
```csharp
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product(command.Name, command.Price, command.Description);
        await _repository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}
```

### Pagination

To support pagination in queries, the following interfaces are used:

- `IHasPagination` – for requests that require pagination
- `IPagedList<T>` – for returning paginated results

### Validation

Input data is validated using the `IValidator<T>` interface, which ensures that commands and queries are checked before execution.

## Best Practices

1. Commands should represent a single, specific operation
2. Queries should return only the necessary data (DTO)
3. Handlers should not call each other directly
4. Use validation to check input data before processing
5. Commands and queries should be immutable after creation

## Example Flow

1. A controller or other component creates a command or query
2. The mediator routes the request to the appropriate handler
3. A validator checks the input data
4. The handler executes the business logic
5. The result is returned to the calling layer

This approach ensures clear separation of concerns and makes the system easier to maintain as complexity grows.
