# Phase 01 — role semantics

**Status:** draft  
**Depends on:** none

## Outcome

The authorization vocabulary contains a stable `RoleNames.Client = "Client"` constant. It represents an imported business customer and is distinct from `User` (generic ordinary user), `Manager`, and `Admin`.

## Work

- Add `Client` to the role-name source of truth and its allowed-role collection.
- Keep role names as stable technical values; use `Клієнт` as the human-readable role description created by the seeder.
- Specify role transition:
  - imported counterparty → ensure `Client`;
  - do not remove `Admin` or `Manager`;
  - remove generic `User` only during the explicit reconciliation of users linked to `Sales.Counterparty`, after compatibility review;
  - do not change unlinked Identity users.

## Acceptance criteria

- [ ] `Client` is available through `RoleNames`, without duplicated string literals.
- [ ] The transition rule is enforced by Infrastructure, not a controller or job runner.

## Verification

- [ ] Unit test role constant/allowed role list if such tests exist.
- [ ] Build Identity Domain.
