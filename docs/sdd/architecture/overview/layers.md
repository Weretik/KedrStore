# Layer boundaries

## Domain

Owns aggregates, entities, value objects, domain errors and domain events. Business invariants belong here and must be enforced by constructors, factories or behaviour methods. Domain has no transport DTO, EF, HTTP, logging, configuration or infrastructure dependency.

Use value objects for validated conceptual identifiers and values. Do not make an entity a public request/response model.

## Application

Owns use cases, commands, queries, handlers, validators and abstractions that adapters implement. Each mediator handler implements one use case. Commands change state; queries do not change state and use no-tracking/read projections where possible.

Application returns Ardalis.Result contracts. It defines persistence/integration interfaces such as read contexts or repositories, but never constructs their implementations.

## Contracts

Module.Contracts is a stable cross-module/client contract boundary for request and response DTOs that need to be consumed outside the module. Keep contracts task-specific; do not use entities as contracts.

## Infrastructure

Owns EF Core DbContexts, entity configurations, migrations, repository implementations, external clients (1C, Telegram), exports, persistence projections and adapter registrations. It may depend on inner layers; inner layers may not depend on it.

## API

Controllers are thin transports: bind input, dispatch one mediator request with the cancellation token, and map Result through the established API mapping. Controllers do not contain business rules, EF queries or transaction orchestration.

## Bootstrapper

Hosts compose modules, configure middleware and register cross-cutting services. They are not a place for use-case logic.
