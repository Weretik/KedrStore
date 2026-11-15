# Backend engineering rules

This section holds stable rules for backend changes. Frontend rules belong to the separate frontend project.

- [Backend rules](backend-rules.md) — layers, CQRS, DDD, Mediator, and code organization.
- [Module rules](module-rules.md) — owning module, when to propose a new module, module shape, schema per module, and FluentValidation.
- [Identifier strategy](identifier-strategy.md) — required ID choice before creating or changing an entity.
- [API rules](api-rules.md) — HTTP contracts, validation, results, and errors.
- [Database rules](database-rules.md) — queries and N+1, indexes and search (pg_trgm per word), transactions, concurrency, and how migrations reach production; known deviations from the 2026-11-15 audit.
- [Idempotency rules](idempotency-rules.md) — where a repeated request or message must not do the work twice, the `Idempotency-Key` pattern, at-least-once handlers (outbox, scheduler, 1C sync, Telegram), tests, and the 2026-11-15 audit of every write operation.
- [Security and observability](security-observability-rules.md) — authorization, privacy, Serilog, and diagnostics.
- [Testing rules](testing-rules.md) — scenario coverage, Red -> Green -> Refactor, test selection, exceptions, and verification commands.
- [Delivery rules](delivery-rules.md) — behavior-focused SDD, traceability, scope, and definition of done.

Feature specifications link to these rules instead of duplicating them. When a rule changes, check related [architecture decisions](../architecture/decisions/README.md) and accepted specifications.
