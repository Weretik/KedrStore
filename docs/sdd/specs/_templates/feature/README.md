# <NNN> — <feature name>

- **Module:** <module>
- **Type:** <feature | migration>
- **Status:** draft
- **Owner:** <team>
- **Created:** YYYY-MM-DD

<One paragraph describing the feature's value and boundary.>

## Requirements

- [Overview and scope](requirements/overview.md)
- [<business capability>](requirements/topic.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)
- [API/integration contract](contracts/api-contract.md)

## Planning and delivery

- [Scenario traceability](traceability.md)
- [Task graph](tasks/README.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)

Behavior tasks use the mandatory lifecycle `Red -> Green -> Refactor ->
Regression -> evidence`. The task graph defines cross-task order through
`Depends on`; phase numbers describe ownership and planning.

## Change notes

- YYYY-MM-DD — <reason, scope impact, and required verification>
