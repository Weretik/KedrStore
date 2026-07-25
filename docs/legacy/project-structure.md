# Project Structure

This project follows the principles of Clean Architecture and adheres to a strict layered separation. Below is an overview of the solution structure and description of each layer.

## Overall Solution Structure

```
ProjectName/
├── src/
│   ├── Domain/
│   ├── Application/
│   ├── Infrastructure/
│   └── WebApi/
├── tests/
│   ├── Domain.UnitTests/
│   ├── Application.UnitTests/
│   ├── Infrastructure.UnitTests/
│   └── WebApi.IntegrationTests/
└── docs/
    ├── architecture/
    ├── dev-guide/
    └── adr/
```

## Layer Descriptions

### Domain

The Domain layer contains core business models, rules, and logic. It is the heart of the application and has no dependencies on other layers or external libraries.

```
Domain/
├── Entities/           # Business entities
├── ValueObjects/       # Value objects
├── Enums/              # Enumerations
├── Exceptions/         # Domain-specific exceptions
├── Events/             # Domain events
└── Interfaces/         # Repository and service interfaces
```

### Application

The Application layer contains application-specific business logic and orchestrates interaction between domain components. It defines contracts (interfaces) to be implemented by external layers.

```
Application/
├── Common/
│   ├── Abstractions/    # Абстракции и интерфейсы
│   │   ├── Common/      # Общие абстракции
│   │   ├── Commands/    # Абстракции для команд
│   │   ├── Events/      # Абстракции для событий
│   │   └── Queries/     # Абстракции для запросов
│   ├── Behaviors/       # MediatR behaviors (logging, validation)
│   ├── Constants/       # Константы приложения
│   ├── Events/          # Реализации для событий
│   ├── Exceptions/      # Application-level exceptions
│   ├── Extensions/      # Методы расширения
│   ├── Mapping/         # AutoMapper profiles and utilities
│   └── Validation/      # Validation utilities
├── DependencyInjection/ # Регистрация зависимостей
├── Interfaces/          # Интерфейсы для инфраструктуры
└── UseCases/            # Случаи использования (по модулям)
    ├── ModuleName1/
    │   ├── Commands/    # Commands (write operations)
    │   └── Queries/     # Queries (read operations)
    └── ModuleName2/
        ├── Commands/
        └── Queries/
```

### Infrastructure

The Infrastructure layer provides concrete implementations of contracts defined in the Application layer for interaction with external resources like databases, file systems, or third-party APIs.

```
Infrastructure/
├── DependencyInjection.cs  # Service registration
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/     # EF Core configurations
│   ├── Migrations/         # Database migrations
│   └── Repositories/       # Repository implementations
├── Identity/               # Authentication implementation
└── Services/               # External service implementations
```

## Testing

The solution includes various test projects to ensure code quality.

```
tests/
├── Domain.UnitTests/         # Domain layer tests
├── Application.UnitTests/    # Application logic tests
├── Infrastructure.UnitTests/ # Infrastructure tests
└── WebApi.IntegrationTests/  # API integration tests
```

## Documentation

The `docs` folder contains project documentation including architecture decisions, developer guides, and more.

```
docs/
├── architecture/          # Architecture overview
├── dev-guide/             # Developer documentation
└── adr/                   # Architecture Decision Records
```
