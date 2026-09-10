# TS-006 — Verify and close customer-read delivery

- **Task ID:** TS-006
- **Covers:** SC-001, SC-002, SC-003, SC-004
- **Depends on:** TS-005
- **Exact paths:** `KedrStore.sln`, `tests/UnitTests/`, `tests/IntegrationTests/`, `docs/sdd/specs/sales/003-customer-read-api/`, `docs/sdd/contracts/`
- **Test level:** Regression and delivery

## Verification evidence

- **Restore:** `dotnet restore KedrStore.sln`; passed, all projects up to date.
- **Build:** `dotnet build KedrStore.sln --no-restore`; passed with 0 warnings and 0 errors.
- **Test:** `dotnet test KedrStore.sln --no-build` after the anonymous-access amendment, with the Sales fixture targeting an isolated PostgreSQL 18 schema; Unit 70/70, Architecture 10/10, Integration 83/83, no skips.
- **Known limitation:** Redocly emits the advisory `info-license` warning for the valid feature contract. Search, writes, history, embedded orders, and exports remain outside the accepted scope.

## Work

- [x] Run restore, build, focused Sales tests, and agreed regression suite.
- [x] Record Red/Green/refactor/regression evidence in every behavior task.
- [x] Mark each traceability scenario with named passing evidence.
- [x] Reconcile requirements, design, data model, contracts, code, and tests.
- [x] Complete delivery checklist and add a delivery report with residual risks.

## Checkpoint

All scenarios have reviewable evidence and delivery documentation matches behavior.
