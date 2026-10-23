# 001 — Database migration job

**Module:** platform
**Type:** migration
**Status:** in progress
**Owners:** backend team
**Created:** 2026-07-29

Move EF Core migration execution from Cloud Run API startup to the root `Host.Jobs` executable. The agreed production sequence resets the noncritical Identity schema and migration history, then recreates them with the current initial migration using Guid keys. API startup must not attempt to change the database schema.

## Scope

- After a fresh backup, reset only the agreed noncritical Identity tables and their historical migration rows.
- Add an explicit one-shot `--job=migrate` mode to `src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj` and its executable code.
- Apply migrations sequentially for all registered contexts, with structured logs and a nonzero exit code on failure.
- Remove automatic migration execution from `Host.Api` startup.
- Update Cloud Run deployment so the migration job succeeds before the new API revision is deployed.

## Out of scope

- Changes to application HTTP contracts or domain behavior.
- Recreating, deleting, or manually editing production Catalog or Sales tables.
- Changes to unrelated 1C import jobs.

## Requirements

- [Overview and scope](requirements/overview.md)
- [Migration safety](requirements/migration-safety.md)

## Technical design

- [Domain and responsibilities](design/domain.md)
- [Infrastructure and deployment](design/infrastructure.md)
- [Agreed Identity SQL reset](design/identity-reset.sql.md)
- [Data model](data-model.md)

## Handoff and verification

- [API/integration contract](contracts/api-contract.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Handoff readiness](checklist/delivery-readiness.md)

## AI implementation tasks

AI completes only unfinished tasks in the current phase, marks completed tasks as `[x]`, and proceeds only after the checkpoint. `[P]` denotes safe parallel execution after dependencies are complete.

| Phase | Outcome |
| --- | --- |
| [00 — Readiness](tasks/00-readiness.md) | Confirmed initial production state and rollback plan |
| [01 — Migration lineage](tasks/01-migration-lineage.md) | Compatible Identity migration history and forward-only change |
| [02 — Root migration job](tasks/02-root-migration-job.md) | Explicit root migration job with safe termination |
| [03 — Deployment coordination](tasks/03-deployment-orchestration.md) | API startup does not change schema; CI runs the job first |
| [04 — API](tasks/04-api.md) | Not applicable; no HTTP contract added |
| [05 — Verification](tasks/05-verification.md) | Evidence for one-shot verification and production deployment |
