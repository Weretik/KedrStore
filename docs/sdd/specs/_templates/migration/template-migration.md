# SDD: <migration name>

**Module:** <module>  
**Status:** draft  
**Migration type:** database | module structure | integration | API contract  
**Owner:** <team/person>

## Goal

Move from <current state> to <target state> without violating the active SDD architecture or breaking documented consumers.

## Context

| Area | Current state | Target state | Owner |
| --- | --- | --- | --- |
| Domain | | | |
| Application | | | |
| Infrastructure | | | |
| API / consumers | | | |
| Data | | | |

## Non-negotiable boundaries

- Dependencies that must not change:
- Public contracts that must remain compatible:
- Data that must not be lost:
- Security/authorization guarantees:
- Explicitly out of scope:

## Target structure

~~~text
<show only folders/files that this migration changes>
~~~

## Phases

1. [Phase 00: discovery and baseline](phases/00-discovery-and-baseline.md)
2. [Phase 01: contract and boundary](phases/01-contract-and-boundary.md)
3. [Phase 02: implementation and data](phases/02-implementation-and-data.md)
4. [Phase 03: rollout and verification](phases/03-rollout-and-verification.md)

Create, remove or split phases only when there is an independently reviewable outcome.

## Rollback

- Trigger:
- Data rollback:
- Application rollback:
- Consumer compatibility during rollback:

## Completion criteria

- [ ] All phase criteria are checked.
- [ ] Restore, build and tests are recorded.
- [ ] Migration/rollout and rollback behaviour are verified.
- [ ] Residual risks are accepted or tracked.

## Change log

- YYYY-MM-DD — Initial draft.
