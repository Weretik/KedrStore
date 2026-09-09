# Delivery rules

## SDD flow

1. Define the goal, scope, business rules, and acceptance scenarios with stable
   `R-*` and `SC-*` IDs.
2. Resolve ambiguous examples, then agree design, data model, risks, and
   contracts only where they have content.
3. Map each scenario to its technical tasks and test level in the traceability
   matrix. Use `TS-*` for implementation tasks and `EN-*` for shared technical
   prerequisites.
4. Implement ready tasks scenario by scenario. Apply Red -> Green -> Refactor
   within each testable technical slice.
5. Verify each completed scenario and then run the relevant regression suite.
6. Update the specification with evidence, deviations, and residual risks.

## Documentation granularity

- One feature has one orchestration `README.md`.
- A document serves one responsibility; do not create empty or duplicate files.
- Acceptance scenarios describe externally observable behavior and avoid
  implementation details.
- Design documents remain the source for technical boundaries and decisions.
- `traceability.md` connects scenarios, technical tasks, and evidence without
  duplicating their detailed content.
- An ADR is required only for a persistent architectural change; ordinary
  use-case decisions belong in the feature design.
- Feature specifications link to `standards/`; do not copy stable backend, API,
  security, or testing rules.

## Definition of done

Every in-scope scenario is implemented or explicitly deferred, its required
technical tasks are complete, and its automated or documented manual evidence
passes. Contracts and implementation agree. Relevant restore/build/test checks
have run or the reason is recorded. The delivery report contains changed files,
verification evidence, unverified work, and residual risks.
