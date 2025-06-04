# Glossary – KedrStore

This glossary defines key architectural and business terms used across the KedrStore project. It ensures shared understanding and consistent communication within the development team.

---

## 🧱 Architectural Terms

- **Module**  
  A self-contained feature or bounded context (e.g., Products, Orders, CRM). Each module has its own domain logic, use cases, and UI components.

- **Layer**  
  A horizontal section in the Clean Architecture structure. Examples: Domain, Application, Infrastructure, Presentation.

- **Entity**  
  A domain object with an identity and lifecycle. Defined in the Domain layer.

- **Value Object**  
  An immutable object that is defined by its value, not identity (e.g., Money, EmailAddress).

- **DTO (Data Transfer Object)**  
  A plain object used to transfer data between layers (e.g., `ProductDto`).

- **Use Case**  
  Application logic representing a user or system action. Implemented as commands and queries.

- **Command**  
  A request to perform an action that changes system state (e.g., `CreateOrderCommand`).

- **Query**  
  A request for data without modifying system state (e.g., `GetOrderByIdQuery`).

- **Handler**  
  A class that processes a specific command or query.

- **Repository**  
  An abstraction for data access, defined in the Domain layer and implemented in Infrastructure.

- **Service**  
  A stateless class that performs logic or delegates between components. Can exist in Domain, Application, or Infrastructure.

- **Validator**  
  A class that verifies correctness of input data using business rules.

- **Middleware**  
  A pipeline component in ASP.NET Core for request/response interception.

- **ErrorBoundary**  
  A Blazor component used to catch and display UI-level errors gracefully.

- **Domain Event**  
  A notification object that represents something that has happened in the domain (e.g., OrderPlacedEvent).
  
- **Business Rule**  
  A validation rule encapsulated as a standalone object in the domain, used to enforce domain invariants.


---

## 🧰 Development Terms

- **MediatR**  
  A .NET library that implements the Mediator pattern for dispatching commands and queries.

- **FluentValidation**  
  A library used for declarative validation of input data.

- **AutoMapper**  
  A tool to map between DTOs and domain models.

- **Serilog**  
  A structured logging library used to capture system and application events.

- **DI (Dependency Injection)**  
  A pattern to inject dependencies into classes via constructors or containers.

- **Result<T>**  
  A type-safe wrapper to represent success/failure outcomes from methods.

---

## 🧪 Testing Terms

- **Unit Test**  
  Tests a single class or method in isolation using mocks/stubs.

- **Integration Test**  
  Tests how multiple components work together (e.g., service + repository + DB).

- **Architecture Test**  
  Validates that the project respects architectural rules (e.g., no cross-layer access).

---

## 💼 Business Terms (project-specific)

- **Product**  
  An item available in the catalog, typically sold in bulk or retail.

- **Order**  
  A confirmed purchase request created by a customer.

- **CRM**  
  Customer Relationship Management – internal tools to manage interactions with clients.

- **Admin (BackOffice)**  
  Internal web panel used by staff to manage products, orders, and customers.

---

## 📁 Folder Naming Conventions

- `Domain/` – core entities, services, and rules  
- `Application/` – use cases, handlers, validators, DTOs  
- `Infrastructure/` – external integrations and persistence  
- `Presentation/` – Blazor pages and components  
- `tests/` – unit, integration, and architecture test projects  
- `docs/` – architectural and technical documentation