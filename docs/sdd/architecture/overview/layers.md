# Layer boundaries

## Domain layer

Contains aggregates, entities, value objects, domain errors, and events. Business invariants belong here and are enforced by constructors, factories, or behavior methods. Domain does not depend on transport DTOs, EF, HTTP, logging, configuration, or Infrastructure.

Use value objects for validated identifiers and values. Do not turn an entity into a public request/response model.

## Application layer

Contains use cases, commands, queries, handlers, validators, and abstractions implemented by adapters. Each Mediator handler performs one use case. Commands change state; queries do not and should use no-tracking reads and projections where possible.

Application returns Ardalis.Result contracts. It defines data persistence and integration interfaces, such as read contexts or repositories, but does not implement them.

## Contracts layer

Module.Contracts is the stable cross-module/client boundary for request and response DTOs consumed outside the module. Contracts must be specific to the use case; do not use entities as contracts.

## Infrastructure layer

Contains EF Core `DbContext`, entity configurations, migrations, repository implementations, external clients (1C, Telegram), exports, persisted-data projections, and adapter registration. It may depend on inner layers; they do not depend on it.

## API layer

Controllers are a thin transport layer: bind input, send one Mediator request with the cancellation token, and map the Result using the existing API conversion. They contain no business rules, EF queries, or transaction coordination.

## Bootstrapper layer

Hosts compose modules, configure middleware, and register cross-cutting services. Use-case logic does not belong here.
