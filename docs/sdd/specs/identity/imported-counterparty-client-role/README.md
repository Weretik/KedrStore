# Imported counterparties receive the Client role

**Module:** cross-module (`Identity` + `Sales`)  
**Status:** draft  
**Owner:** KedrStore backend  
**Created:** 2026-07-26  
**Related:** [counterparty and Identity import flow](../../../architecture/integrations/one-c/sales-counterparty-identity-sync.md)

## Goal

As a customer imported from 1C, I want my linked Identity account to have the `Client` role so that authorization can distinguish customers from ordinary site users and staff.

## Baseline

Current declared role constants are `Admin`, `Manager`, `User`, and `Guest`. The current role seeder creates `Admin`, `Manager`, and `User`; `Guest` is declared but is not seeded. The imported-counterparty provisioning service currently ensures only `User`.

## Scope

- In scope:
  - Add a seeded system role whose stable technical name is `Client` and whose display description is `Клієнт`.
  - Grant `Client` to every Identity account provisioned for a 1C counterparty.
  - Migrate the current imported-counterparty path from the generic `User` role to `Client`.
  - Make repeated imports idempotent: no duplicate role assignment and no loss of staff roles.
  - Define the handling of existing imported counterparties that currently have `User`.
- Out of scope:
  - New login/registration endpoints.
  - New client-facing API endpoints or frontend UI.
  - Changing permissions/policies unless a later feature needs client-only access.
  - Changing the 1C counterparty contract.
- Assumptions:
  - Role names are technical English values; Ukrainian is stored as the role description, not as the role key.
  - Counterparties are identified by `Sales.Counterparty.IdentityUserId`.
  - `Client` is the least-privilege business role for accounts imported from 1C.
- Dependencies:
  - Identity database migration/seeding lifecycle.
  - `IImportedCounterpartyIdentityProvisioningService` implementation.
  - A one-time reconciliation of existing imported users.

## Scenarios

1. Given 1C returns a valid new counterparty, when `counterparties` runs, then the created `AppUser` has `Client` and the Sales counterparty links to that user.
2. Given a known counterparty is imported again, when the job runs, then the same user still has exactly one `Client` assignment and no duplicate link is created.
3. Given an existing imported counterparty user with `User`, when the rollout reconciliation runs, then it receives `Client` and its generic `User` role is removed only if it is an imported-only role assignment.
4. Given an imported user has `Admin` or `Manager`, when a counterparty is reimported, then those staff roles remain untouched.
5. Given the `Client` role is missing because database seeding has not run, when provisioning is attempted, then the job logs a controlled failure/skip; it must not silently succeed without the required role.

## Contract and compatibility

- API contract document: not needed; no endpoint or response shape changes.
- Existing consumers and compatibility: existing `User`-based authorization remains unchanged. A future endpoint may explicitly require `Client`; that is a separate API/security feature.
- Authorization and idempotency: assignment is role-based and idempotent. `Client` must not grant Admin/Manager permissions. No HTTP write endpoint is added.

## Implementation phases

| Phase | Status | Required? | Outcome |
| --- | --- | --- | --- |
| [01 Domain](phases/01-domain.md) | draft | yes | `Client` role name and role-transition rule are explicit |
| [02 Infrastructure](phases/02-infrastructure.md) | draft | yes | seed, provisioning, existing-user reconciliation and tests |
| 03 Application | not needed | no | existing provisioning abstraction is sufficient; no Mediator use case changes |
| 04 API | not needed | no | no route, controller or OpenAPI change |
| [05 Manual verification](phases/05-manual-verification.md) | draft | yes | repeatable DB/job validation |
| 06 Frontend handoff | not needed | no | no frontend contract change |

## Files

- Create:
  - `docs/sdd/specs/identity/imported-counterparty-client-role/` specification files.
  - Migration only if Identity role persistence requires it; adding a seeded role normally changes data, not the Identity schema.
- Modify:
  - `Identity.Domain.Authorization.RoleNames`.
  - `Identity.Infrastructure.Seeders.RoleSeeder`.
  - `Identity.Infrastructure.Services.ImportedCounterpartyIdentityProvisioningService`.
  - Tests for role provisioning/reconciliation, if test projects cover Identity/Sales integration.
- Do not change:
  - Generated OneC SOAP client.
  - Password/source-field logic as part of this role-only feature.
  - Existing Admin/Manager authorization policies.

## Acceptance criteria

- [ ] The Identity database contains the seeded `Client` role with a `Клієнт` description.
- [ ] New valid 1C counterparties receive `Client` during user provisioning.
- [ ] Reimport does not duplicate the role or counterparty link.
- [ ] Existing imported counterparties are reconciled to `Client` according to the documented migration rule.
- [ ] `Admin` and `Manager` assignments are never removed by the import.
- [ ] A missing required role is observable and does not produce an apparently successful customer import.

## Verification

- Unit: provisioning role-selection and idempotency tests.
- Integration: Identity/Sales context test for an imported counterparty and existing user.
- Architecture: dependency direction remains `Sales Infrastructure → Identity Application abstraction`; Sales does not reference Identity Infrastructure directly.
- Manual: [phase 05](phases/05-manual-verification.md).
- Commands: `dotnet restore`, targeted `dotnet test`, `dotnet build`.

## Risks and open questions

- [ ] Confirm whether pre-existing imported users must retain `User` in addition to `Client`. This draft proposes removing only `User` from users positively identified as imported counterparties; it never removes staff roles.
- [ ] Decide whether `Guest` should be seeded in a separate cleanup task; it is not part of this feature.
- [ ] Confirm whether any production authorization policy currently requires `User` for customer access before role removal is enabled.

## Change log

- 2026-07-26 — Initial draft based on current Identity role seed and 1C counterparty provisioning code.
