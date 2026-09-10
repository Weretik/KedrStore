# TS-006 — Verify and close manager-order read delivery

- **Task ID:** TS-006
- **Covers:** SC-001, SC-002, SC-003, SC-004, SC-005
- **Depends on:** TS-005
- **Exact paths:** `KedrStore.sln`, `tests/UnitTests/`, `tests/IntegrationTests/`, `docs/sdd/specs/sales/002-manager-order-read-api/`, `docs/sdd/contracts/`
- **Test level:** Regression and delivery

## Verification evidence

- **Restore:** `dotnet restore KedrStore.sln`; passed, all projects up to date.
- **Build:** `dotnet build KedrStore.sln --no-restore`; passed with 0 warnings and 0 errors.
- **Test:** `dotnet test KedrStore.sln --no-build` with `KEDR_SALES_TEST_POSTGRES_CONNECTION` targeting an isolated PostgreSQL 18 schema; Unit 59/59, Architecture 10/10, Integration 67/67, no skips.
- **Known limitation:** TS-001's initially authored Red could not execute because Docker was unavailable; the gap is documented in TS-001. Focused and full Green/regression provider checks later passed against local PostgreSQL 18. Repository-wide aggregate Redocly lint retains pre-existing errors in the older admin-order sync contract; the new feature contract validates and its aggregate/runtime references are tested.

## Work

- [x] Run restore, build, focused Sales tests, and agreed regression suite.
- [x] Record Red/Green/refactor/regression evidence in every behavior task.
- [x] Mark each traceability scenario with named passing evidence.
- [x] Reconcile requirements, design, data model, contracts, code, and tests.
- [x] Complete delivery checklist and add a delivery report with residual risks.

## Checkpoint

All scenarios have reviewable evidence and delivery documentation matches behavior.
