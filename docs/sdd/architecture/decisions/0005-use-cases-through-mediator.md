# ADR-0005: Every use case is a command or query sent through Mediator

- Status: accepted
- Date: 2026-11-15 (recorded from the current code; the approach has been in use since the modular layout of 2026)
- Owner: Виталий (project owner)

## Context

The backend has more than 170 use cases across seven modules. Each one needs the same cross-cutting steps: logging, input validation, timing, error handling and domain events. Endpoints used to hold logic of their own, and the same steps were repeated by hand. One developer must be able to open any use case and find the same shape.

## Alternatives considered

- Services with many methods per area (`CatalogService`): rejected; services grow without limit and every method repeats validation and logging.
- MediatR (used by the 2025 version, see `docs/legacy/adr`): replaced by the source-generated `Mediator` library, which has the same request/handler model without runtime reflection.
- Controllers: replaced by Minimal API endpoints, one file per endpoint, so the HTTP layer stays as thin as the use case it calls.

## Decision

- Every use case is a command (changes data) or a query (reads data) with one handler, in `Application/Features/<Feature>/Commands|Queries/<UseCase>/` next to its validator and response.
- An endpoint maps its own HTTP request type to a separate command or query (a request is never the command), sends it through `Mediator` 3.x and maps the result through the one result mapper ([ADR-0006](0006-result-and-typed-errors.md)).
- Cross-cutting steps are pipeline behaviors in a fixed order: request logging, exception, performance, validation, domain event dispatch ([mediator behaviors](../runtime/mediator-behaviors.md)).
- A command handler only orders the steps: load, check what needs outside data, call the aggregate's method, one `SaveChanges`, return an id or a DTO.

## Why

- Every use case looks the same and can be found by its folder name.
- Validation, logging and timing are written once.
- Reads and writes are separate, so a query can use a lean projection without touching aggregates.

## Trade-offs

- A small use case still needs three or four files (request, command, handler, validator).
- The pipeline order matters; a new behavior must be placed with care.

## Sources and checks

- [Use-case flow](../runtime/use-case-flow.md), [module rules](../../standards/module-rules.md), [technology stack](../platform/technology-stack.md).
- Architecture tests check folder names and layer references (`tests/ArchitectureTests`).
