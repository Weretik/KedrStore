# Feature task graph

This file indexes the feature's planned tasks. Replace template links with the
actual files selected for the feature and remove unused templates.

## Identifiers

- `R-001`: business rule in `requirements/`;
- `SC-001`: acceptance scenario in `requirements/`;
- `TS-001`: one implementation or verification responsibility;
- `EN-001`: one shared or prerequisite responsibility.

Checklist IDs inside a task are local steps. `TS-*` and `EN-*` are the stable
identifiers used by `traceability.md`.

## Planning phases

- [00 — Behavior and readiness](00-readiness.md)
- [01 — Domain tasks](01-domain.md)
- [02 — Infrastructure and enablers](02-infrastructure.md)
- [03 — Application tasks](03-application.md)
- [04 — API and contract tasks](04-api.md)
- [05 — Verification and delivery](05-verification.md)

The phase numbers organize planning. Implementation follows task dependencies
scenario by scenario; it does not require completing an entire layer first.

## Selected technical tasks

| ID | Responsibility | File |
| --- | --- | --- |
| TS-001 | <technical responsibility> | <relative link> |
| EN-001 | <shared prerequisite> | <relative link> |

Each task file contains exact paths, its test level, work, evidence, and a
checkpoint. Execute the graph with
`docs/sdd/specs/_templates/ai-feature-workflow/README.md` and
`docs/sdd/standards/testing-rules.md`.
