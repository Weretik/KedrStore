# Phase 02: Infrastructure and persistence

**Status:** draft  
**Depends on:** phase 01, unless the feature is read-only over an existing model  
**Blocks:** application persistence use cases

## Outcome

The feature is persisted and integrated using the existing module infrastructure conventions.

## Design

- DbContext and DbSet impact:
- EF IEntityTypeConfiguration:
- indexes, constraints and concurrency:
- repository/read-context implementation:
- read projection or no-tracking query strategy:
- external adapter or DI registration:
- migration: required | not required, with reason:
- data backfill/projection rebuild, if required:

## File plan

~~~text
<Module>.Infrastructure/
├── DataBase/
├── Configurations/
├── Migrations/                 only when approved
├── Repositories/
├── Projections/
├── Integrations/
└── DependencyInjection/
~~~

## Acceptance criteria

- [ ] Domain/Application do not reference Infrastructure.
- [ ] Read paths avoid tracking and N+1 queries where applicable.
- [ ] Migration and rollback impact are recorded, or no-migration rationale exists.
- [ ] Adapter registrations follow the module extension pattern.

## Verification

- Integration/persistence test:
- Migration/projection check:
- Risks:
