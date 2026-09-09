# Test architecture — checklist: delivery readiness

- [x] Every in-scope scenario is verified, with the documented local Docker exception.
- [x] Unit tests reference only intended inner-layer assemblies.
- [x] PostgreSQL concurrency verification is implemented and mandatory in Docker-enabled CI; local unavailability is an accepted exception.
- [x] Architecture rules execute and report actionable violations.
- [x] Existing API and adapter integration tests remain green.
- [x] Task evidence and justified exceptions are recorded.
- [x] Restore, build, and all locally executable tests pass.
- [x] Coverage is collected locally and configured for mandatory CI artifact upload.
- [x] Documentation, implementation, test names, and workflow agree.
- [x] All task IDs are closed or carry a concrete blocker.
