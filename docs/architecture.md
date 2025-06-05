# KedrStore Architecture Overview

## Overview

KedrStore applies the principles of **Clean Architecture** to ensure long-term maintainability, testability, and flexibility of the codebase. The architecture is structured into multiple layers, organized around the **Dependency Rule** — dependencies point inward, toward the core business logic.

---

## Core Principles

1. **Framework Independence** – Business logic does not depend on any external frameworks
2. **Testability** – All components can be tested in isolation
3. **UI Independence** – The user interface can be changed without affecting the rest of the system
4. **Database Independence** – The database can be swapped without modifying business logic
5. **External Service Independence** – Business logic is protected from changes in third-party APIs

---

## Layered Structure

### 1. Domain Layer (`KedrStore.Domain`)
The innermost and most stable layer containing:
- Business entities
- Domain events (e.g., `BaseDomainEvent`)
- Repository interfaces
- Domain services
- Domain-specific exceptions
- Enums and domain constants
- Base abstractions (`IEntity`, `IAggregateRoot`, `IDomainEvent`, `IHasDomainEvents`)
- Base classes (`BaseEntity`, `ValueObject`, `BusinessRule`, `RuleChecker`)

🧱 This layer has **no dependencies** on other layers or external libraries.

---

### 2. Application Layer (`KedrStore.Application`)
Contains application-specific business logic:
- Use cases (commands and queries)
- Service interfaces
- DTOs (Data Transfer Objects)
- Object mapping definitions
- Input validation
- Application-level rules

🔗 Depends only on the Domain Layer.

---

### 3. Infrastructure Layer (`KedrStore.Infrastructure`)
Implements integrations with external systems:
- Repository implementations
- Database access logic
- External API integrations
- Email sending logic
- Logging
- Caching
- Authentication & Authorization

🔗 Depends on Domain and Application Layers.

---

### 4. Presentation Layer (`KedrStore.Presentation`)
Responsible for user-facing components:
- Blazor Web App components and pages
- UI-level services
- Routing configuration
- Error handling (UI level)
- Middleware

🔗 Depends on Application Layer and optionally Infrastructure Layer.

---

## Data Flow

1. **Request**: A user interacts with the UI (Presentation Layer)
2. **Handling**: The UI triggers a use case in the Application Layer
3. **Business Logic**: The Application Layer processes the request and interacts with Domain entities
4. **Data Access**: If needed, the Application Layer uses the Infrastructure Layer to read/write data
5. **Response**: The result is returned back to the Presentation Layer and shown to the user

---

## Layer Interactions

### Domain → Application
- Application layer uses domain entities and repository interfaces

### Application → Infrastructure
- Infrastructure implements interfaces defined in the Application or Domain
- Follows **Dependency Inversion Principle (DIP)**

### Application → Presentation
- Presentation invokes use cases via DI
- Translates DTOs into ViewModels or UI data

### Диаграмма зависимостей слоёв

```
┌─────────────────┐
│  Presentation   │
│    (UI Layer)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐     ┌─────────────────┐
│   Application   │     │ Infrastructure   │
│  (Use Cases)    │◄────┤ (Implementation) │
└────────┬────────┘     └─────────────────┘
         │
         ▼
┌─────────────────┐
│     Domain      │
│ (Business Logic)│
└─────────────────┘
```

The direction of the arrows indicates the dependencies between the layers. Note that Infrastructure depends on Application and Domain, but not vice versa - this is an implementation of the dependency inversion principle.

---

## Design Patterns & Architecture Styles

1. **CQRS (Command Query Responsibility Segregation)**
    - Commands for state changes
    - Queries for data retrieval
    - Handled using MediatR

2. **Repository Pattern**
    - Interfaces in Domain
    - Implementations in Infrastructure

3. **Mediator Pattern**
    - MediatR used for dispatching commands and queries
    - Promotes decoupling of handlers

4. **Factory Pattern**
    - Used for constructing complex objects
    - Encapsulates creation logic

5. **Unit of Work**
    - Groups operations into single transactional unit
    - Used for atomic data changes

---

## Database Design

### Master Tables

- **Products**: Store Products
- **Categories**: Product categories
- **Orders**: Customer Orders
- **OrderItems**: Order Items
- **Customers**: Customer Data
- **Users**: System users (administrators, managers)
- **Baskets**: Shopping baskets
- **BasketItems**: Basket items

### Data schema (simplified)

```
+---------------+      +---------------+      +---------------+
|   Category    |      |    Product    |      |   OrderItem   |
+---------------+      +---------------+      +---------------+
| CategoryId PK |<---->| ProductId PK  |<---->| OrderItemId PK|
| Name          |      | Name          |      | ProductId FK  |
| Description   |      | Description   |      | OrderId FK    |
| ParentId FK   |      | Price         |      | Quantity      |
+---------------+      | CategoryId FK |      | UnitPrice     |
                       | Stock         |      +---------------+
                       | ImageUrl      |              |
                       +---------------+              |
                                                      |
                       +---------------+              |
                       |    Order      |<-------------+
                       +---------------+
                       | OrderId PK    |
                       | CustomerId FK |
                       | OrderDate     |
                       | Status        |
                       | TotalAmount   |
                       +---------------+
                                |
                                |
                       +---------------+
                       |   Customer    |
                       +---------------+
                       | CustomerId PK |
                       | Name          |
                       | Email         |
                       | Phone         |
                       | Address       |
                       +---------------+
```

A complete detailed database schema with indexes, constraints, and triggers is available in the file [docs/architecture/db-schema.md](/docs/architecture/db-schema.md).

---

## Technology Stack

| Area               | Technology                         |
|--------------------|-------------------------------------|
| Language           | C# 12                               |
| Framework          | .NET 8.0                            |
| UI                 | Blazor Web App                      |
| ORM                | Entity Framework Core               |
| Validation         | FluentValidation                    |
| Mapping            | AutoMapper                          |
| DI Container       | Microsoft.Extensions.DependencyInjection |
| Logging            | Serilog                             |
| Testing            | xUnit, Moq, FluentAssertions        |

---

## Deployment

### Containerization

The project comes with a Docker-ready configuration for all components:

- **Web App**: .NET 8 Blazor Web App
- **Database**: PostgreSQL in a separate container
- **Cache**: Redis for caching and sessions

### Docker Compose

For local development and testing, `docker-compose.yml` is used to bring up all necessary services in an isolated environment.

### CI/CD Pipeline

The repository is configured to work with GitHub Actions:

1. **Continuous Integration**: Tests and code quality checks are run on every PR
2. **Continuous Deployment**:
   - Building Docker images
   - Publishing to Azure Container Registry
   - Updating Kubernetes cluster via Helm

### Infrastructure

- **Production**: Azure Kubernetes Service (AKS)
- **Staging**: Azure App Service
- **Development**: Local Developer Environment
A detailed deployment and configuration diagram is available in [docs/architecture/deployment.md](/docs/architecture/deployment.md).

---

## Security

- Authentication: ASP.NET Identity, JWT tokens
- Authorization: Role-based & Policy-based access
- Protection against CSRF, XSS, and common vulnerabilities
- Encryption for sensitive data

---

## Performance

- Caching strategies (in-memory, Redis)
- Asynchronous processing
- Query optimizations

---

## Scalability

- Horizontal scaling (load balancing)
- Vertical scaling (resource upgrade)
- Data partitioning strategies

---

## Monitoring & Logging

- Structured logging with Serilog
- Request tracing and diagnostics
- Performance metrics
- Critical error alerts

---

## Related Documentation

- [Naming Conventions](./naming-conventions.md)
- [Module Guidelines](./module-guidelines.md)
- [Error Handling](./error-handling.md)
- [Architectural Decisions (ADR)](./adr/0001-use-clean-architecture.md)  