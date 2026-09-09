# <feature name> — checklist: specification readiness

- [ ] Scope and observable behavior are complete and do not contradict each other.
- [ ] Every changed business rule has a stable `R-*` ID.
- [ ] Every in-scope behavior has a stable `SC-*` scenario ID.
- [ ] Each rule has a happy-path and relevant negative or boundary examples.
- [ ] Given/When/Then statements contain no implementation details.
- [ ] Every `[NEEDS CLARIFICATION]` is resolved or explicitly out of scope.
- [ ] Each scenario has a design solution or a deferred status.
- [ ] The data model contains no speculative data or relationships.
- [ ] Contract, security, idempotency, and integration dependencies are agreed.
- [ ] For changed HTTP behavior, the human-readable contract and planned
      machine-readable OpenAPI path are defined before transport implementation.
- [ ] `traceability.md` maps every scenario to its test level and `TS-*`/`EN-*`
      tasks; tasks do not exceed scope.
- [ ] Every task has exact paths, dependencies, a checkpoint, and planned
      Red/Green/regression evidence or a justified test-first exception.
