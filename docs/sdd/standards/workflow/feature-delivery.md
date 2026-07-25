# Feature delivery and SDD

## Required sequence

1. Create a feature folder from specs/_templates/feature/README.md and copy only applicable phase templates.
2. Write the goal, scope, scenarios, contracts, layer placement, risks and acceptance criteria.
3. Split work into numbered phases only when phases can be reviewed or delivered independently.
4. Implement only approved/in-scope work.
5. Update the specification with verification evidence and residual risks.

## Documentation granularity

- One feature has one overview specification.
- A phase gets its own file when it has a distinct outcome, dependency or verification.
- An ADR is required only for a durable architectural decision; do not create ADRs for ordinary use-case work.
- Link instead of copying rules. A specification must not duplicate the layer or security standard.

## Definition of done

The acceptance criteria are checked, relevant tests are updated, restore/build/test were attempted, and the result states what could not be verified and why.
