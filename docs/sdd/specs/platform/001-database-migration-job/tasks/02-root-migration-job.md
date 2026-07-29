# Phase 02 — Root migration job

- [ ] T008 Add a dedicated migration orchestration service in `src/Bootstrapper/Host.Jobs/Host.Jobs/` that invokes registered `IDatabaseMigrator` instances in the approved deterministic order. **Dependencies:** T004.
- [ ] T009 Add `--job=migrate` dispatch to `src/Bootstrapper/Host.Jobs/Host.Jobs/Program.cs`; it must require no root IDs, propagate cancellation, log context-safe progress, and return non-zero on failure. **Dependencies:** T008.
- [ ] T010 Confirm `src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj` contains all required project references and publish content for the new migration job; add only missing root-job dependencies. **Dependencies:** T008.
- [ ] T011 Add unit/integration coverage for command dispatch, ordering, failure stop, and idempotent re-execution. **Dependencies:** T009, T010.

## Checkpoint

The migration job must be runnable locally against a disposable database before deployment orchestration changes.
