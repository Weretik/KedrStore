# TS-005 — Repository verification

- **Covers:** SC-001, SC-002, SC-003, SC-004, SC-005
- **Depends on:** TS-004
- **Exact paths:** `KedrStore.sln`, `tests`, `.github/workflows/deploy-cloudrun.yml`
- **Test level:** regression

## Verification evidence

- Restore command and result: `dotnet restore KedrStore.sln` passed.
- Build command and result: `dotnet build KedrStore.sln --no-restore --configuration Release` passed with 28 pre-existing compiler and package-vulnerability warnings.
- Unit command and result: Release plus `XPlat Code Coverage` passed 41 tests.
- Integration command and result: Release plus `XPlat Code Coverage` passed 44 tests and skipped 1 because Docker is unavailable.
- Architecture command and result: Release plus `XPlat Code Coverage` passed 10 tests after the final dependency rule was added.
- Known limitation: Docker is not installed on the current workstation; the
  PostgreSQL test is expected to report a descriptive skip locally and run in CI.

## Work

- [x] Run `dotnet restore KedrStore.sln`.
- [x] Run `dotnet build KedrStore.sln --no-restore --configuration Release`.
- [x] Run every test project with coverage collection.
- [x] Inspect the final diff and update traceability and readiness evidence.

## Checkpoint

All locally executable checks pass and the Docker limitation is explicit.
