# Phase 01: Domain model and invariants

**Status:** draft  
**Depends on:** accepted parent SDD  
**Blocks:** infrastructure and application work

## Outcome

The domain model can represent the business capability without invalid state.

## Design

- Aggregate root/entity:
- Value objects and typed IDs:
- Required fields and nullability:
- Invariants and state transitions:
- Domain errors:
- Domain events, if a completed business fact must be published:

## File plan

~~~text
<Module>.Domain/
├── Entities/<Area>/
├── ValueObjects/
├── Errors/
└── Events/
~~~

## Acceptance criteria

- [ ] Invalid values cannot create or mutate the aggregate.
- [ ] IDs/value objects enforce their own conceptual validation.
- [ ] No EF, HTTP, DTO, controller or infrastructure dependency is introduced.
- [ ] Domain events are collected only when required.

## Verification

- Unit tests for factories, transitions and invariants:
- Manual/domain review:
- Risks:
