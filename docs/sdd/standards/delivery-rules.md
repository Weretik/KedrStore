# Delivery rules

Prepare an SDD before implementing a new feature or changing the scenarios, API contracts, database schema, or job behavior of an existing feature. For an existing feature, update its current specification or add a revision in its directory; do not duplicate the entire SDD. Maintain traceability for the changed scope.

For a local internal fix that restores already documented behavior without changing a scenario, contract, database schema, or module boundary, a short description of the problem, fix, and focused verification in the task or report is enough. Read historical revisions only for a specific need.

## SDD workflow

Keep a feature's current scope and navigation in its `README.md`, requirements in the current specification, evidence in the corresponding tasks, and traceability as brief links and statuses. Preserve earlier revisions as history; do not copy current status into them or open them without a specific need.

1. Define the goal, scope, business rules, and acceptance scenarios with stable `R-*` and `SC-*` IDs.
2. Resolve ambiguous examples, then agree on design, data model, risks, and contracts only where they add value. Before implementation, add a compact planned map of key files by scenario and layer to the SDD: exact path, one role, existing or planned status, and a decision about splitting any affected overloaded files.
3. Link each scenario to technical tasks and a test level in the traceability matrix. Use `TS-*` for implementation tasks and `EN-*` for shared technical prerequisites.
4. Complete ready tasks scenario by scenario. Apply Red → Green → Refactor to each testable technical slice.
5. Verify each completed scenario, then run the relevant regression checks.
6. Update the specification with evidence, deviations, and residual risks.
7. After implementing a new feature, add its current structure to the [feature map](../features/README.md) and its index; when changing an existing feature, review and update the description in the same change where needed.

## Documentation granularity

A new feature needs a concise description of its actual architecture in `docs/sdd/features/<module>/` and an index link. The description includes a tree of key files and their roles, a compact Mermaid diagram of the verified flow between entry point, handlers, model, data, and integrations, and a “Tests” section with the related test files and a focused run command (or a note that none exist). Keep environment state, such as whether a migration has been applied, out of the card. Update the map after a substantial change to these facts; an internal detail that does not affect it needs no repeated description. The SDD records decisions and evidence; the map describes the current implementation.

- One feature SDD has one coordinating `README.md`; a brief description of the actual implementation lives separately in `features/`.
- Each document has one role; do not create empty or duplicate files.
- Acceptance scenarios describe externally observable behavior without implementation details.
- Design documents remain the source for technical boundaries and decisions.
- `traceability.md` links scenarios, technical tasks, and evidence without duplicating their content.
- An ADR is needed only for a lasting architecture change; ordinary use-case decisions belong in the feature design.
- Feature specifications link to `standards/` rather than copying stable backend, API, security, or testing rules.
- When tests for a feature are added, moved, or removed, update the card's “Tests” section in the same change.
- When a YAML file in `docs/sdd/contracts/` changes, say in the PR that kedr-web needs `npm run contracts:sync -- <path-to-KedrStore>`.

## Completion criteria

Every scenario in the agreed scope is implemented or explicitly deferred; its required technical tasks are complete, and automated or documented manual verification passed. Contracts match the implementation. New and changed files are checked against the [backend rules](backend-rules.md): independent scenarios and roles are separated, and the planned map is compared with the actual tree in the feature map; file size alone does not require splitting. Focused tests for the changed feature and related components are run, or the reason they could not be run is recorded; general tests and a full suite run only at the user's explicit request under the [testing rules](testing-rules.md). The delivery report includes changed files, verification evidence, unrun checks, and residual risks.
