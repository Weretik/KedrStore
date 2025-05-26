# Module Guidelines – KedrStore

## Purpose

Define common structure, naming, and interaction principles for all feature-based modules in KedrStore.

---

## Module Definition

Each module represents a distinct feature or domain boundary (e.g., Products, Orders, CRM).

A module must:

- Be **isolated** and follow Clean Architecture
- Have its own folder structure under each layer
- Communicate with other modules via interfaces or MediatR

---

## Standard Folder Structure per Module

Example for `Products`:

```
/src
├── Domain/
│   └── Products/
│       ├── Product.cs
│       ├── IProductRepository.cs
│       └── ProductCreated.cs
├── Application/
│   └── Products/
│       ├── Commands/
│       │   └── CreateProductCommand.cs
│       ├── Queries/
│       │   └── GetProductByIdQuery.cs
│       ├── DTOs/
│       │   └── ProductDto.cs
│       └── Handlers/
│           └── CreateProductCommandHandler.cs
├── Infrastructure/
│   └── Products/
│       ├── ProductRepository.cs
│       ├── ProductConfiguration.cs
│       └── ProductDbContextSeeder.cs
├── Presentation/
│   └── Products/
│       ├── Pages/
│       │   └── ProductList.razor
│       └── Components/
│           └── ProductCard.razor
```

---

## Naming Rules

- Folder names must match module name (e.g., `Products`, `Orders`)
- Use consistent casing: PascalCase for folders and files
- Handler: `{CommandName}Handler`
- Query: `{Action}{Entity}Query`
- Command: `{Action}{Entity}Command`
- ViewModel: `{Entity}ViewModel` or `{Entity}VM`

---

## Interaction Between Modules

- Prefer **interfaces** or **MediatR** for communication
- Avoid direct calls between Application/Infrastructure modules
- Domain events may be used for loose coupling (e.g., `ProductCreatedEvent`)

---

## Dependencies

- Modules in `Domain`, `Application`, `Infrastructure` **must not depend on each other** unless explicitly defined
- `Presentation` can reference multiple modules, but must use Application interfaces only

---

## Versioning and Isolation

- Each module should be versionable and testable independently
- Avoid shared state or static classes across modules

---

## Test Structure

- `/tests/UnitTests/{Module}/...`
- `/tests/IntegrationTests/{Module}/...`

Each module must have:
- Use case tests
- Repository tests
- Integration validation (optional)

---

## Violations

Any deviation must be documented and approved via ADR.

---

## Future Enhancements

- Generator templates for modules
- Dynamic module loading (optional)
- Cross-cutting concerns registration per module