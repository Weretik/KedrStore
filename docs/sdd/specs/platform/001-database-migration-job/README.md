# 001 — Database migration job

**Module:** platform
**Type:** migration
**Status:** draft
**Owner:** backend team
**Created:** 2026-07-29

Move EF Core schema migration execution out of the Cloud Run API startup path and into the root `Host.Jobs` executable. Restore the deployed Identity migration lineage so production PostgreSQL recognizes its existing Identity tables, then create only forward migrations for real schema changes. The API must start without attempting to alter a database schema.

## Scope

- Restore source-controlled Identity migration IDs that match the production `__EFMigrationsHistory` baseline.
- Add an explicit, one-shot `--job=migrate` mode to `src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj` and its executable.
- Run migrations serially for all registered contexts with structured logs and a non-zero exit code on failure.
- Remove automatic migration execution from `Host.Api` startup.
- Update the Cloud Run deployment workflow so the migration job completes successfully before a new API revision is deployed.

## Out of scope

- Changing application HTTP contracts or domain behavior.
- Recreating, deleting, or manually editing production tables.
- Changing unrelated 1C import jobs.

## Requirements

- [Overview and scope](requirements/overview.md)
- [Migration safety](requirements/migration-safety.md)

## Technical design

- [Domain and ownership](design/domain.md)
- [Infrastructure and rollout](design/infrastructure.md)
- [Data model](data-model.md)

## Delivery

- [API/integration contract](contracts/api-contract.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## AI implementation tasks

AI performs only unfinished tasks in the current phase, marks completed tasks as `[x]`, and proceeds only after the checkpoint. `[P]` indicates safe parallel execution after dependencies are complete.

| Phase | Result |
| --- | --- |
| [00 — Readiness](tasks/00-readiness.md) | confirmed production baseline and rollback plan |
| [01 — Migration lineage](tasks/01-migration-lineage.md) | compatible Identity migration history and forward-only delta |
| [02 — Root migration job](tasks/02-root-migration-job.md) | explicit root migration job with safe exit behavior |
| [03 — Deployment orchestration](tasks/03-deployment-orchestration.md) | API startup is schema-neutral; CI runs the job first |
| [04 — API](tasks/04-api.md) | not applicable; no HTTP contract is introduced |
| [05 — Verification](tasks/05-verification.md) | disposable and production rollout evidence |
