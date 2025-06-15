# Domain Layer

This document describes the structure and implementation principles of the **Domain Layer** in the `KedrStore` project, developed according to **DDD (Domain-Driven Design)** and **Clean Architecture** principles.

---

## 🎯 Purpose of the `Domain` Layer

The `Domain` layer contains the **core business logic**, including:
- Entities and their behavior
- Value Objects (VO)
- Aggregates and Aggregate Roots
- Domain Events
- Business rules and abstractions (e.g., `IBusinessRule`, `IEntity`, `IAggregateRoot`)
- Domain-specific exceptions

It has **no dependencies on infrastructure, databases, or frameworks** — only pure .NET logic.

---

## 📁 Structure

```
Domain/
├── Abstractions/           # IEntity, IAggregateRoot, etc.
├── Catalog/                # Catalog module: categories, products, events
│   ├── Entities/           # Product, Category
│   ├── ValueObjects/       # ProductId, CategoryId, Money
│   ├── Events/             # ProductCreatedEvent, CategoryCreatedEvent
│   └── Repositories/       # IProductRepository, ICategoryRepository
├── Identity/               # Role definitions, Scope, AccessLevel
├── Events/                 # Base domain events
├── Errors/                 # Domain exceptions and error types
├── Common/                 # Shared types and validation rules
└── DomainAssemblyMarker.cs
```

---

## 🧩 Key Concepts

### 📌 Entities
- Have a unique identity (`Id`) and behavior
- Inherit from `Entity<T>` and implement `IEntity`
- Examples: `Product`, `Category`, `AppUser`

### 📌 Value Objects
- No identity, compared by value
- Immutable
- Examples: `Money`, `CategoryId`, `ProductId`

### 📌 Aggregates
- Group related Entities and ValueObjects under one root
- All changes go through the Aggregate Root
- Example: `Product` as an aggregate

### 📌 Domain Events
- Implement `IDomainEvent`
- Raised via `AddDomainEvent()` inside an entity
- Examples: `ProductCreatedEvent`, `RoleAssignedEvent`

### 📌 Domain Exceptions
- Signal business rule violations
- Extend from `DomainException`

---

## 🛠 Design Rules

- ❌ No dependencies on MediatR, EF Core, infrastructure, etc.
- ✅ Pure business logic only
- New entities should follow:
  - Entity + VO + Id
  - Exception handling for invalid state
  - Emit domain event if needed

---

## 🧪 Testing

Domain layer is easy to unit-test:
- No mocks or infrastructure
- Focused on behavior and invariants

---

## 🧱 Extension Guide

To add a new domain feature:

1. Create entity in the appropriate module
2. Add a typed `Id` as a ValueObject if needed
3. Validate state with `Ensure.That()` or rules
4. Use `AddDomainEvent()` to raise events
5. Cover with unit tests

---

## 📎 Examples

### Entity
```csharp
public class Product : Entity<ProductId>, IAggregateRoot
{
    public string Name { get; private set; }
    public Money Price { get; private set; }
    // ...other properties...
    public Product(ProductId id, string name, Money price)
    {
        Id = id;
        Name = name;
        Price = price;
        // Raise domain event
        AddDomainEvent(new ProductCreatedEvent(id));
    }
}
```

### Value Object
```csharp
public record Money(decimal Amount, string Currency)
{
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Currency mismatch");
        return new Money(Amount + other.Amount, Currency);
    }
}
```

### Aggregate Root
```csharp
public class Category : Entity<CategoryId>, IAggregateRoot
{
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products;
    // ...methods to add/remove products...
}
```

### Domain Event
```csharp
public record ProductCreatedEvent(ProductId ProductId) : IDomainEvent;
```

### Domain Exception
```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

### Business Rule Example
```csharp
public class ProductNameMustBeUniqueRule : IBusinessRule
{
    private readonly string _name;
    private readonly IProductRepository _repository;
    public ProductNameMustBeUniqueRule(string name, IProductRepository repository)
    {
        _name = name;
        _repository = repository;
    }
    public bool IsBroken() => _repository.ExistsByName(_name);
    public string Message => "Product name must be unique.";
}
```

### Unit Test Example
```csharp
[Fact]
public void Product_Created_With_Valid_Data_Should_Raise_Event()
{
    var id = new ProductId(Guid.NewGuid());
    var product = new Product(id, "Test", new Money(100, "RUB"));
    Assert.Contains(product.DomainEvents, e => e is ProductCreatedEvent);
}
```

## 📝 Naming & Organization Tips
- Use the `Id` suffix for Value Object identifiers (e.g., `ProductId`, `CategoryId`).
- Domain exceptions should inherit from `DomainException`.
- Business rules should implement the `IBusinessRule` interface.
- All changes to aggregates must go through the Aggregate Root.
- Keep domain logic free from infrastructure dependencies.
- Place shared validation logic in the `Common` folder.
- Use clear, intention-revealing names for entities, value objects, and events.
- Organize domain modules by business subdomain (e.g., `Catalog`, `Identity`).

---

This documentation can be extended to fit your team's processes and standards. If you need examples for other patterns or modules, let us know!
