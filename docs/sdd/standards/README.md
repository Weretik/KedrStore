# Backend engineering rules

This section holds stable rules for backend changes. Frontend rules belong to the separate frontend project.

- [Backend rules](backend-rules.md) — layers, CQRS, DDD, Mediator, and code organization.
- [Identifier strategy](identifier-strategy.md) — required ID choice before creating or changing an entity.
- [API rules](api-rules.md) — HTTP contracts, validation, results, and errors.
- [Database rules](database-rules.md) — EF Core, reads, transactions, migrations, and data.
- [Security and observability](security-observability-rules.md) — authorization, privacy, Serilog, and diagnostics.
- [Testing rules](testing-rules.md) — scenario coverage, Red -> Green -> Refactor, test selection, exceptions, and verification commands.
- [Delivery rules](delivery-rules.md) — behavior-focused SDD, traceability, scope, and definition of done.

Feature specifications link to these rules instead of duplicating them. When a rule changes, check related [architecture decisions](../architecture/decisions/README.md) and accepted specifications.
