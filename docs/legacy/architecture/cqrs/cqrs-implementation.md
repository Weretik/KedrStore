# CQRS Implementation in KedrStore

## Overview

The CQRS (Command Query Responsibility Segregation) pattern in KedrStore is implemented using the MediatR library and a system of abstractions, ensuring a clear separation between read and write operations.

## Implementation Principles

1. **Strict separation** – commands and queries are never mixed
2. **Single handler** – every command or query has exactly one handler
3. **Transparency** – all operations go through the mediator
4. **Validation** – all inputs are validated
5. **Behaviors** – MediatR behaviors are used for cross-cutting concerns

## Abstraction Structure

### Base Interfaces

```
IUseCase
   |
   ├── ICommand<TResult>    # Commands with result
   |      |
   |      └── ICommand      # Commands without result (Unit)
   |
   └── IQuery<TResult>      # Queries with result
```

## Commands

Commands represent the intent to change system state. They implement `ICommand<TResult>` or `ICommand` for void commands.

### Command Interfaces

```csharp
public interface ICommand<out TResult> : IUseCase { }
public interface ICommand : ICommand<Unit> { }

public readonly struct Unit
{
    public static readonly Unit Value = new();
}
```

### Example Command with Result

```csharp
public class CreateProductCommand : ICommand<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}
```

### Example Command without Result

```csharp
public class DeleteProductCommand : ICommand
{
    public int Id { get; set; }
}
```

### Command Handler Example

```csharp
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateProductCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
```

## Queries

Queries retrieve data without modifying system state. They implement `IQuery<TResult>`.

### Query Interface

```csharp
public interface IQuery<out TResult> : IUseCase { }
```

### Query Examples

```csharp
public class GetProductByIdQuery : IQuery<ProductDto>
{
    public int Id { get; set; }
}

public class GetProductsWithPaginationQuery : IQuery<PaginatedList<ProductDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
```

### Query Handler Example

```csharp
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), request.Id);
        }

        return _mapper.Map<ProductDto>(product);
    }
}
```

## Behaviors

MediatR behaviors allow injection of cross-cutting concerns into the pipeline.

### Validation Behavior

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

### Logging Behavior

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId ?? string.Empty;

        _logger.LogInformation("Handling {RequestName} for user {UserId}", requestName, userId);

        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName} for user {UserId}", requestName, userId);
            throw;
        }
    }
}
```

### Domain Event Dispatcher Behavior

```csharp
public class DomainEventDispatcherBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IApplicationDbContext _dbContext;

    public DomainEventDispatcherBehavior(IDomainEventDispatcher dispatcher, IApplicationDbContext dbContext)
    {
        _domainEventDispatcher = dispatcher;
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse response = await next();

        var domainEvents = _dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        _dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .ToList()
            .ForEach(e => e.Entity.ClearDomainEvents());

        await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return response;
    }
}
```

## Registration

```csharp
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatcherBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
        services.AddAutoMapper(typeof(ApplicationAssemblyMarker));
        services.AddDomainEventHandlers(typeof(ApplicationAssemblyMarker).Assembly);

        return services;
    }
}
```

## Controller Usage Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var query = new GetProductByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result }, result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var command = new DeleteProductCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
```

## CQRS Recommendations

1. **Structure**:
   - Organize commands and queries by module in `UseCases`
   - Each module should have `Commands` and `Queries` folders
   - One class per file

2. **Naming**:
   - Commands: Verb + Command (e.g., `CreateProductCommand`)
   - Queries: Question-style + Query (e.g., `GetProductByIdQuery`)
   - Handlers: Match with request type (e.g., `CreateProductCommandHandler`)

3. **API Style**:
   - Commands mutate state, return minimal results (typically ID)
   - Queries are read-only and return DTOs

4. **Pagination**:
   - Use `PaginatedList<T>` and standard params (`pageNumber`, `pageSize`)

5. **Validation**:
   - Use FluentValidation for every request
   - Register `ValidationBehavior`

6. **Caching**:
   - Use `CachingBehavior` where needed
   - Define policy per query

7. **Transactions**:
   - Use `UnitOfWork` to ensure atomicity within command handlers

## Conclusion

CQRS is a cornerstone of the KedrStore architecture, promoting clear separation of concerns. A proper implementation leads to maintainable, testable, and scalable code.
