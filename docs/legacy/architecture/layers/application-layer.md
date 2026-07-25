# Application Layer

This document describes the structure and implementation principles of the **Application Layer** in the `KedrStore` project, following **Clean Architecture** and **DDD** best practices.

---

## 🎯 Purpose of the `Application` Layer

The `Application` layer coordinates business use cases and orchestrates domain logic. It is responsible for:
- Use cases (commands, queries, CQRS)
- Application services and handlers
- Transaction management
- Validation and authorization
- Mapping between domain and DTOs
- Defining interfaces for infrastructure (e.g., repositories, external services)

It contains no direct business logic or infrastructure code, only orchestration and coordination.

---

## 📁 Structure

```
Application/
├── UseCases/               # Commands, queries, handlers
├── Interfaces/             # Abstractions for repositories, services
├── Common/                 # Shared logic, base classes, validation
├── Extensions/             # DI registration, configuration
├── ApplicationAssemblyMarker.cs
```

---

## 🧩 Key Concepts

### 📌 Use Cases (CQRS)
- Commands: change state (e.g., `CreateProductCommand`)
- Queries: read data (e.g., `GetProductByIdQuery`)
- Handlers: implement use case logic, call domain and infrastructure

### 📌 Application Services
- Coordinate multiple domain operations
- Implement transactional workflows
- Orchestrate domain events and notifications

### 📌 Validation & Authorization
- Use validation libraries (e.g., FluentValidation)
- Implement authorization checks before executing use cases

### 📌 Mapping
- Map between domain models and DTOs (e.g., with AutoMapper)

### 📌 Abstractions
- Define interfaces for repositories, external services, etc.
- Infrastructure layer provides implementations

---

## 🛠 Design Rules

- ❌ No direct business logic or infrastructure code
- ✅ Only orchestration, coordination, and application logic
- Application depends on Domain, but not vice versa
- Use dependency injection for all external dependencies
- Keep use cases focused and single-responsibility

---

## 🧪 Testing

- Application layer is tested with unit and integration tests
- Mock domain and infrastructure dependencies
- Focus on use case behavior and orchestration

---

## 🧱 Extension Guide

To add a new application feature:

1. Define a command/query and its handler in `UseCases/`
2. Add validation and authorization if needed
3. Use interfaces for infrastructure dependencies
4. Register handlers and services in DI
5. Cover with unit and integration tests

---

## 📎 Examples

### Command and Handler
```csharp
public record CreateProductCommand(string Name, decimal Price) : IRequest<Guid>;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;
    public CreateProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }
    public async Task<Guid> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var product = new Product(new ProductId(Guid.NewGuid()), command.Name, new Money(command.Price, "RUB"));
        await _repository.AddAsync(product);
        return product.Id.Value;
    }
}
```

### Validation Example
```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

### Interface Example
```csharp
public interface IProductRepository
{
    Task AddAsync(Product product);
    Task<Product?> GetByIdAsync(ProductId id);
    // ...other methods...
}
```

### DI Registration Extension
```csharp
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(ApplicationAssemblyMarker).Assembly);
        // ...register validators, mappers, etc...
        return services;
    }
}
```

---

## 📝 Naming & Organization Tips
- Use clear, use-case-driven names for commands, queries, and handlers
- Place shared logic in the `Common` folder
- Define interfaces in `Interfaces` and implement them in Infrastructure
- Register all handlers and services via DI extension methods
- Organize by business subdomain if needed

---

This documentation can be extended to fit your team's processes and standards. If you need examples for other patterns or modules, let us know!

