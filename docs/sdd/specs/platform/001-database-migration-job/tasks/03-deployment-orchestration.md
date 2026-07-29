# Phase 03 — API startup and deployment orchestration

- [x] T012 Remove automatic migration execution from `src/Bootstrapper/Host.Api/Program.cs` and separate any remaining seeding behavior explicitly. **Dependencies:** T009.
- [x] T013 Update `.github/workflows/deploy-cloudrun.yml` to update/execute a dedicated `database-migrate` Cloud Run Job with the Jobs image and database secret, wait for completion, then deploy the API. **Dependencies:** T009, T012.
- [ ] T014 Verify Cloud Run IAM and service-account permissions follow least privilege for the migration job, Artifact Registry, Cloud SQL, and Secret Manager. **Dependencies:** T013.
- [x] T015 Add deployment-level checks that prevent API rollout when the migration job exits non-zero. **Dependencies:** T013.

## Completion record (2026-07-29)

- API startup no longer invokes migrations or seeders.
- The workflow updates or creates `database-migrate`, runs `--job=migrate` with `--wait`, and only deploys the API after that command succeeds.
- T014 remains open because Cloud Run and hosting IAM cannot be inspected from the current local account.

## Checkpoint

The API may be deployed only after the migration job succeeds for the same image revision and target database.
