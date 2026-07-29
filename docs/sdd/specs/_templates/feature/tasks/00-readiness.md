# Phase 00 — Clarification and readiness

- [ ] T001 Read requirements, design, data model, contracts, and the specification checklist; record contradictions without making assumptions.
- [ ] T002 Resolve every `[NEEDS CLARIFICATION]` item or move it out of scope.
- [ ] T003 Agree the consumer contract before implementing consumer code. For HTTP, create `docs/sdd/contracts/<module>/<feature>.openapi.yaml` and reference it from `docs/sdd/contracts/openapi.yaml`; for non-HTTP integrations, document the agreed contract in `contracts/api-contract.md`.
- [ ] T004 Mark the applicable items in the specification-readiness checklist.

## Checkpoint

Do not start Domain until the model and contract decisions are made.
