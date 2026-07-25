# AI working context

Read this file before planning a non-trivial backend change, then follow the linked documents.

1. Read docs/sdd/architecture/README.md and docs/sdd/standards/workflow/feature-delivery.md.
2. Create or update a specification under docs/sdd/specs before implementation.
3. Keep dependencies inward: API → Application → Domain. Infrastructure implements inner abstractions.
4. Use Mediator for each use case, FluentValidation for input and Ardalis.Result for expected outcomes.
5. Put business invariants in Domain; do not place them in controllers, EF configurations or pipeline behaviours.
6. Treat public API contracts as stable. Document an additive or breaking contract decision in the specification.
7. Use AsNoTracking/projections for reads and preserve transaction, authorization, domain-event and logging conventions.
8. Run restore, build and test; report exact failures and whether they are pre-existing.

For a new feature, first read docs/sdd/specs/_templates/feature/README.md and copy its target structure. Use docs/sdd/specs/_templates/migration/template-migration.md for a migration and docs/sdd/specs/_templates/git/git-commit-batching.md to plan commits.

For local startup, configuration, migrations/seeders, Swagger diagnostics or OneC jobs, read docs/sdd/operations/README.md before acting.
