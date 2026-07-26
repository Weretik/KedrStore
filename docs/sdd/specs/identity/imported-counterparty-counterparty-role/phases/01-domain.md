# Phase 01 — role vocabulary

**Status:** completed  
**Depends on:** none

## Outcome

Add `RoleNames.Counterparty = "Counterparty"` and include it in the central allowed-role catalogue. It represents an imported business counterparty and is distinct from `User`, `Manager`, and `Admin`.

## Transition rule

- Ensure `Counterparty` for every account provisioned from a 1C counterparty.
- Never remove `Admin` or `Manager`.
- Remove generic `User` only through an explicit, audited reconciliation of users linked from `Sales.Counterparty` and only after checking policy compatibility.
- Never change roles of unlinked Identity users.

## Acceptance criteria

- [x] No duplicated role-name literals outside the role source of truth.
- [x] `Counterparty` is clearly least-privilege and does not inherit staff permissions.

## Verification result

- No dedicated role-catalogue unit test project was found.
- `dotnet build src/Identity/Identity.Domain/Identity.Domain.csproj --no-restore`
