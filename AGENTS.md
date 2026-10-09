# KedrStore: instructions for AI

These instructions apply to the whole repository.

## Repository navigation

1. Read `docs/sdd/architecture/README.md` before changing module or layer boundaries.
2. Read the relevant files from `docs/sdd/standards/README.md`. Before creating or changing an entity identifier, read `docs/sdd/standards/identifier-strategy.md`.
3. For a new feature or migration, create or update its specification under `docs/sdd/specs/` before implementation.
4. For public HTTP behavior, follow `docs/sdd/contracts/README.md` and keep the feature contract, versioned OpenAPI, runtime behavior, and tests aligned.
5. For startup, configuration, migrations, diagnostics, or jobs, read `docs/sdd/operations/README.md`.
6. Use `docs/product/glossary.md` for stable product terminology.

## Architecture and delivery

- Preserve the modular-monolith boundaries: business modules do not depend on one another's internals, and dependencies point toward Domain.
- Keep business rules out of controllers, EF mappings, hosts, and `BuildingBlocks`.
- Implement accepted behavior through dependency-ready `TS-*` and `EN-*` tasks. Apply Red -> Green -> Refactor -> Regression and record fresh evidence.
- A normal `Host.Api` start applies migrations and seeders. Treat startup and database changes as state-changing operations and never point them at an unapproved database.
- Keep documentation and contracts in the same change set as the behavior they describe.
- Preserve unrelated user changes in a dirty worktree.

## AI execution scope

When the user authorizes a feature, phase, scenario, or ordered task set, continue through all ready in-scope work without requesting a separate command for every task. Stop at the authorized boundary, an unresolved product decision, a documented blocker, or an action requiring new authority.

## Git

- Do not commit, push, create a pull request, or alter remote state unless the user explicitly requests it.
- When commits are requested, follow `docs/sdd/specs/_templates/git/git-commit-batching.md`: use small reviewable commits by intent, keep inseparable model/configuration/migration changes together, and do not include unrelated worktree changes.

## Repo-local skills

Use the matching skill from `.agents/skills/` when its trigger applies:

- `kedrstore-feature-spec` — create or update a feature specification before code.
- `kedrstore-feature-delivery` — implement an accepted specification.
- `kedrstore-feature-orchestrator` — coordinate explicitly requested multi-agent delivery.
- `kedrstore-change-verification` — select and run risk-appropriate checks.
- `kedrstore-code-audit` — audit architecture and implementation evidence.
- `kedrstore-remediation-plan` — plan systemic architecture remediation.
- `kedrstore-api-contract-sync` — keep HTTP behavior and OpenAPI aligned.
- `kedrstore-database-change` — design and verify EF Core migration or seed changes.
- `kedrstore-pr-handoff` — prepare a review-ready handoff.
- `kedrstore-docs-sync` — find and repair confirmed documentation drift.

## Documentation rules

- `docs/sdd/architecture/` and `docs/sdd/standards/` contain durable rules; feature-specific decisions belong in `docs/sdd/specs/<module>/<NNN>-<feature>/`.
- Keep machine-readable public API contracts in `docs/sdd/contracts/`.
- One document serves one responsibility. Do not create empty, duplicate, or monolithic files.
- Verify relative Markdown links after moving or renaming files.
