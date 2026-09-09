# TS-004 — Gate deployment and publish coverage

- **Covers:** SC-005
- **Depends on:** TS-002, TS-003
- **Exact paths:** `.github/workflows/deploy-cloudrun.yml`
- **Test level:** workflow configuration

## Work

- [x] Add a verification job before deployment.
- [x] Restore and build the solution once.
- [x] Run unit, architecture, and PostgreSQL integration tests.
- [x] Collect Cobertura coverage and upload the results artifact.
- [x] Make deployment depend on successful verification.

## Test-first exception and replacement verification

- Reason: GitHub Actions orchestration cannot be meaningfully Red-tested by the local test suite.
- Replacement check: parse YAML, inspect job dependency, and run equivalent local commands where Docker permits.
- Result: `yaml-lint` passed; equivalent local Release commands created three
  Cobertura reports. The workflow configuration is accepted as delivery
  evidence; runtime CI failures, if any, are handled separately.

## Checkpoint

No Cloud Run deployment step can begin when verification fails.
