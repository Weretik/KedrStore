# EN-002 — Host.Jobs and Cloud Run photo-check wiring

- **Task ID:** EN-002
- **Enables:** SC-010
- **Depends on:** TS-004
- **Exact paths:** `src/Bootstrapper/Host.Jobs/Host.Jobs/Program.cs`, `src/Bootstrapper/Host.Jobs/Host.Jobs/JobsHostServicesExtensions.cs`, `src/Bootstrapper/Host.Jobs/Host.Jobs/appsettings.json`, `.github/workflows/deploy-cloudrun.yml`, `docs/sdd/operations/jobs/host-jobs-cli.md`, `docs/sdd/operations/jobs/catalog-photo-check-runbook.md`
- **Test level:** CLI integration + workflow static verification

## Why this is an enabler

Host dispatch and deployment YAML expose the already tested Application job to
operators. Their correctness is verified through command dispatch and static
deployment configuration rather than reproducing photo behavior.

## Work

- [x] Register the job, options, and adapters in the jobs composition root.
- [x] Map only `--job=check-product-photos`; require no `rootId`.
- [x] Document local execution, summary interpretation, cancellation, and the operator-owned schedule.
- [x] Add create-or-update logic for `catalog-product-photo-check` using the agreed image, argument, secret, task count, parallelism, retry, and timeout.
- [x] Prove the workflow contains no execution command for this job and no scheduler creation.

## Test-first exception and replacement verification

- Reason Red-first is not meaningful: Cloud Run provisioning cannot be safely executed from the automated test suite, and the wiring adds no new business classification behavior.
- Replacement command/check: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter FullyQualifiedName~ProductPhotoCheckDeploymentTests`; Release build of Host.Jobs; manual workflow diff inspection. `actionlint` and `yamllint` were queried but are not installed in the local environment.
- Result: static deployment test passed 1/1 and asserts the exact command, job name, secret, execution limits, and absence of job execution or scheduler creation; Release solution build passed with zero warnings and errors.

## Checkpoint

The shared image can run the one-shot command locally and in Cloud Run, while
deployment only creates or updates its definition.
