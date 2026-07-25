# <Feature name>

**Module:** <catalog | sales | identity | platform | cross-module>  
**Status:** draft  
**Owner:** <team/person>  
**Created:** YYYY-MM-DD  
**Related:** <issue, ADR, frontend spec, API consumer>

## Goal

As a <role>, I want <action> so that <outcome>.

## Scope

- In scope:
- Out of scope:
- Assumptions:
- Dependencies:

## Scenarios

1. Given <precondition>, when <action>, then <observable result>.

## Contract and compatibility

- API contract document: contracts/api-contract.md | not needed because:
- Existing consumers and compatibility:
- Authorization and idempotency:

## Implementation phases

| Phase | Status | Required? | Outcome |
| --- | --- | --- | --- |
| [01 Domain](phases/01-domain.md) | draft | yes/no | model, IDs, invariants and domain events |
| [02 Infrastructure](phases/02-infrastructure.md) | draft | yes/no | EF persistence, configuration and integration registration |
| [03 Application](phases/03-application.md) | draft | yes/no | use cases, validators, specifications and results |
| [04 API](phases/04-api.md) | draft | yes/no | controllers, routes, authorization and OpenAPI |
| [05 Swagger manual test](phases/05-swagger-manual-test.md) | draft | yes/no | repeatable manual verification |
| [06 Frontend handoff](phases/06-frontend-handoff.md) | draft | yes/no | stable frontend contract and usage guidance |

Mark a phase not needed only with a short reason. Each implemented phase records its own verification result.

## Files

- Create:
- Modify:
- Do not change:

## Acceptance criteria

- [ ]

## Verification

- Unit:
- Integration:
- Architecture:
- Manual:
- Commands:

## Risks and open questions

- [ ]

## Change log

- YYYY-MM-DD — Initial draft.
