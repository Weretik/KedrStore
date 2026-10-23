# BuildingBlocks guide

BuildingBlocks is the shared technical foundation. It defines cross-module primitives and implementations but does not own Catalog, Sales, or Identity business rules.

## Project map

~~~text
src/BuildingBlocks/
├── BuildingBlocks.Domain/            entity, auditing, error, and event primitives
├── BuildingBlocks.Application/       Mediator behaviors, contracts, notifications, and logging
├── BuildingBlocks.Infrastructure/    shared adapters, migrations, and seeders
├── BuildingBlocks.Api/               Ardalis.Result MVC mapping
└── BuildingBlocks.Integrations.OneC/ shared SOAP client factory and authentication
~~~

## Reading order

1. [Domain primitives](domain.md)
2. [Application primitives and behaviors](application.md)
3. [Infrastructure services and startup](infrastructure.md)
4. [API result mapping](api.md)
5. [1C integration foundation](one-c.md)
6. [Usage rules](usage-rules.md)

Consult this guide before creating a new shared base type, behavior, or service abstraction.
