# Manager order read API — checklist: specification readiness

- [x] Scope is limited to read projections of existing Sales orders, lines, sync state, and counterparty relation.
- [x] Rules R-001 through R-006 and scenarios SC-001 through SC-005 are stable identifiers.
- [x] The existing order graph and no-side-effect boundary are documented.
- [x] No speculative entity, identifier, or migration is introduced.
- [x] Traceability covers every in-scope scenario.
- [x] Routes, access, fields, pagination/filtering, and historical/deleted-customer behavior are resolved or excluded.
- [x] Human-readable API contract, error mapping, and planned OpenAPI operation details are agreed.
- [x] Each planned task is expanded with exact paths, dependencies, checkpoints, and planned Red/Green/regression evidence.
