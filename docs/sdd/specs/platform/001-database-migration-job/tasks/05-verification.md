# Phase 05 — Verification and rollout

- [x] T017 Run `dotnet restore KedrStore.sln`, `dotnet build KedrStore.sln --no-restore`, and relevant tests; record outcomes and pre-existing failures. **Dependencies:** T011, T015.
- [ ] T018 Apply the approved Identity reset and current Guid initial migration to a disposable database matching the production baseline; verify only Identity tables are re-created. **Dependencies:** T006, T011.
- [ ] T019 Run `Host.Jobs --job=migrate` twice against the disposable database; verify the second run is a successful no-op. **Dependencies:** T009, T018.
- [ ] T020 Deploy and execute the Cloud Run `database-migrate` Job after a verified Cloud SQL backup; record sanitized logs and confirm API revision starts and listens on its assigned port. **Dependencies:** T003, T013, T019.
- [ ] T021 Complete `checklist/delivery-readiness.md`, align the specification with code, and record rollback evidence and residual risks. **Dependencies:** T017-T020.

## Verification record (2026-07-29)

- `dotnet restore KedrStore.sln` succeeded with existing NuGet vulnerability warnings.
- `dotnet build KedrStore.sln --no-restore` succeeded with 48 existing warnings and no errors.
- Focused migration tests passed: 3/3. Full solution tests passed 21/22 unit tests and 4/4 integration tests; the existing failure is `UnitTests.SlugTests.MapProduct_Should_Clean_Slugs`.
- T018-T021 remain open until the reset is run against a disposable database and the hosting backup/IAM/Cloud Run steps are verified.
