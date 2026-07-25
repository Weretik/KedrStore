# BuildingBlocks reference

BuildingBlocks is the shared technical foundation. It defines cross-module primitives and implementations, but never owns Catalog, Sales or Identity business rules.

## Project map

~~~text
src/BuildingBlocks/
├── BuildingBlocks.Domain/            entity, audit, error and event primitives
├── BuildingBlocks.Application/       Mediator behaviors, contracts, notifications and logging
├── BuildingBlocks.Infrastructure/    shared adapter implementations, migrations and seeders
├── BuildingBlocks.Api/               Ardalis.Result to MVC mapping
└── BuildingBlocks.Integrations.OneC/ shared SOAP client factory and authentication behavior
~~~

## Read in this order

1. [Domain primitives](domain.md)
2. [Application primitives and behaviors](application.md)
3. [Infrastructure services and startup](infrastructure.md)
4. [API result mapping](api.md)
5. [1C integration foundation](one-c.md)
6. [Usage rules and availability](usage-rules.md)

Do not create another shared base type, behavior or service abstraction before checking this reference.
