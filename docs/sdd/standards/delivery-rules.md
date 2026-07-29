# Delivery rules

## SDD flow

1. Create a feature specification with the goal, scope, scenarios, contracts, risks, and acceptance criteria.
2. Add design, data model, contracts, and quality checklist documents only when they have content.
3. Split implementation into numbered phases and atomic tasks when they can be reviewed or delivered independently.
4. Implement only the agreed work within scope.
5. Update the specification with verification evidence and residual risks.

## Documentation granularity

- One feature has one orchestration `README.md`.
- A document serves one responsibility; do not create empty or duplicate files.
- An ADR is required only for a persistent architectural change; ordinary use-case decisions belong in the feature design.
- Feature specifications link to `standards/`; do not copy stable backend, API, security, or testing rules.

## Definition of done

Acceptance criteria are met, relevant tests are updated, and restore/build/test have run or the reason is recorded. The delivery report contains changed files, verification evidence, what could not be verified, and residual risks.
