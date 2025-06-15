# ADR 0008: Define Domain Structure for Catalog Module

## Status
Accepted

## Date
2025-06-15

## Context
The `Catalog` module is one of the core modules of KedrStore and requires a well-structured, domain-driven layout to ensure long-term scalability, maintainability, and clarity.

The Domain layer is the innermost and most stable layer in Clean Architecture. It must encapsulate all business rules, entities, value objects, events, and repository contracts — independently from any frameworks or technologies.

## Decision
We define the following structure for the `Domain/Catalog` module:

```
Domain/
└── Catalog/
    ├── Entities/
    │   ├── Category.cs
    │   └── Product.cs
    │
    ├── ValueObjects/
    │   ├── CategoryId.cs
    │   ├── ProductId.cs
    │   └── Money.cs
    │
    ├── Events/
    │   ├── CategoryCreatedEvent.cs
    │   └── ProductCreatedEvent.cs
    │
    ├── Repositories/
    │   ├── ICategoryRepository.cs
    │   └── IProductRepository.cs
```

### Naming
- Entities are singular, no suffixes (`Category`, not `CategoryEntity`)
- Events follow `{Entity}CreatedEvent` naming
- Repository interfaces follow the convention `I{Entity}Repository`
- Value objects are self-describing, immutable, and comparable

### Base Abstractions
Additionally, the following shared abstractions are located in `Domain/Common` or `Domain/Abstractions`:
- `BaseEntity.cs`
- `ValueObject.cs`
- `IAggregateRoot.cs`
- `IDomainEvent.cs`

All entities implement `BaseEntity` and (where applicable) `IAggregateRoot`.  
All domain events implement `IDomainEvent`.

## Consequences

### Positive
- Domain logic is encapsulated and framework-independent
- Repository contracts are clearly separated from implementation
- Domain events enable decoupled workflows
- Value objects improve type-safety and self-validation

### Negative
- Slightly higher onboarding complexity
- Requires explicit mapping in Application and Infrastructure

## Notes
- Domain must **not reference** Application or Infrastructure
- No external dependencies allowed in this layer
- All business invariants must be enforced inside entities or value objects
