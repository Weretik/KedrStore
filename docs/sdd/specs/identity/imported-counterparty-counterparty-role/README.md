# Imported counterparties receive the Counterparty role

**Module:** cross-module (`Identity` + `Sales`)  
**Status:** in progress  
**Owner:** KedrStore backend  
**Created:** 2026-07-26  
**Related:** [counterparty and Identity import flow](../../../architecture/integrations/one-c/sales-counterparty-identity-sync.md)

## Goal

As a customer imported from 1C, I want my linked Identity account to have the `Counterparty` role so that authorization can distinguish a 1C business counterparty from ordinary users and staff.

## Baseline and target role catalogue

Current declared roles are `Admin`, `Manager`, `User`, and `Guest`. The current seed creates `Admin`, `Manager`, and `User`; `Guest` is declared but not seeded. The 1C counterparty importer currently ensures `User`.

This feature adds the following role to the central `RoleNames` catalogue and Identity seed:

| Technical role | Display description | Intended holder | Seeded by default |
| --- | --- | --- | --- |
| `Counterparty` | `Контрагент` | Identity account linked to `Sales.Counterparty` and provisioned from 1C | yes |

The resulting declared role catalogue will be `Admin`, `Manager`, `User`, `Guest`, `Counterparty`.

## Scope

- In scope:
  - Add and seed `Counterparty` as a system role.
  - Grant it to every user provisioned by the current 1C counterparty import.
  - Replace generic `User` with `Counterparty` for imported-only accounts, using a safe explicit reconciliation.
  - Preserve staff roles and make repeated imports idempotent.
- Out of scope:
  - New login, registration, API, frontend or 1C SOAP contract.
  - New policies or permissions; a later feature may explicitly require `Counterparty`.
  - Seeding `Guest`; that is a separate cleanup decision.

## Scenarios

1. Given a valid new 1C counterparty, when `counterparties` runs, then its linked `AppUser` receives `Counterparty`.
2. Given the same counterparty is imported again, then it has one `Counterparty` assignment and no duplicate Sales link.
3. Given an existing imported account with `User`, when approved reconciliation runs, then it gains `Counterparty`; generic `User` is removed only from a user positively linked to a Sales counterparty.
4. Given an imported account also has `Admin` or `Manager`, when reimported or reconciled, then those staff roles remain unchanged.
5. Given the `Counterparty` role is missing, when provisioning runs, then the counterparty is skipped with an observable error; the import must not report success without the required role.

## Contract and compatibility

- API contract: not needed; no endpoint or response changes.
- Compatibility: existing `User` policies remain unchanged. Any policy migration from `User` to `Counterparty` is a separate authorization feature.
- Idempotency: role assignment must be repeat-safe and must not duplicate `AspNetUserRoles` rows.

## Implementation phases

| Phase | Status | Required? | Outcome |
| --- | --- | --- | --- |
| [01 Domain](phases/01-domain.md) | completed | yes | stable role vocabulary and transition rule |
| [02 Infrastructure](phases/02-infrastructure.md) | draft | yes | seed, provisioning, reconciliation and tests |
| 03 Application | not needed | no | current provisioning abstraction is sufficient |
| 04 API | not needed | no | no controller, route or OpenAPI change |
| [05 Manual verification](phases/05-manual-verification.md) | draft | yes | repeatable safe verification |
| 06 Frontend handoff | not needed | no | no frontend contract change |

## Acceptance criteria

- [ ] `Counterparty` appears in `RoleNames.All` and is seeded with description `Контрагент`.
- [ ] New valid 1C counterparties receive `Counterparty`.
- [ ] Existing linked imported accounts are reconciled according to the documented transition rule.
- [ ] `Admin` and `Manager` are never removed by this import feature.
- [ ] Missing role failures and reconciliation counts are observable without secrets.

## Risks and open questions

- [ ] Confirm before implementation whether existing imported accounts should temporarily keep `User` in addition to `Counterparty`. This draft proposes removing only `User` from positively identified imported accounts after compatibility review.
- [ ] Confirm no existing customer endpoint is protected solely by `User` before that removal is enabled.

## Change log

- 2026-07-26 — Renamed planned role from `Client` to `Counterparty`; added it to the target role catalogue.
- 2026-07-26 — Phase 01 completed: `RoleNames.Counterparty` added to the central role catalogue.
