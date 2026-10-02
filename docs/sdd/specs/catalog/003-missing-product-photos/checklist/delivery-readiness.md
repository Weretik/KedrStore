# Missing product photos — checklist: delivery readiness

- [x] Every in-scope `SC-*` row in `traceability.md` is verified or explicitly deferred.
- [x] Domain state transitions and URL-staleness boundaries are tested.
- [x] Job orchestration, result classification, paging, validation, and result behavior are tested.
- [x] PostgreSQL constraints, migration/backfill, and index are verified; physical provider lifecycle checks are owned by CI.
- [x] CDN adapter verifies HEAD, fallback GET, media type, timeout, redirects, and bounded body handling.
- [x] The anonymous API conforms to the agreed OpenAPI contract.
- [x] Host.Jobs exposes the documented command and cancellation behavior.
- [x] Cloud Run workflow creates or updates the job without executing or scheduling it.
- [x] Every behavior task records Red, Green, refactor, regression, and evidence.
- [x] Every enabler records its exception reason and replacement verification.
- [x] Restore, build, test, workflow validation, and focused checks pass or have documented ownership.
- [x] Operations documentation, contracts, implementation, and names agree.
- [x] All task IDs are closed and residual risks have an owner and next step.

Residual verification ownership: CI must run the three PostgreSQL lifecycle
tests with Docker available. Optional `actionlint`/`yamllint` checks remain a CI
tooling concern; the repository static workflow test verifies the feature's
exact deployment behavior.
