# Phase 06 — Verification and rollout

**Status:** draft

## Outcome

Deploy the migration and import changes safely, verify catalog output, then remove frontend duplication only after the new contract is live.

## Work

- Run migration before the first import using the new metadata.
- Run one non-overlapping full 1C import.
- Validate doors, hardware and Cosmos categories, levels, labels and ordering against the approved configuration.
- Confirm public/admin catalog behavior and monitor import logs.
- Coordinate frontend removal of constants only after API adoption.

## Acceptance criteria

- [ ] Migration, import and projection complete successfully in the target environment.
- [ ] No category disappears unexpectedly.
- [ ] Ordering and labels match the approved source mapping.

## Verification

- `dotnet restore`, `dotnet build`, `dotnet test`, migration check, full-job log review and manual API/frontend validation.
