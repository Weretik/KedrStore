# Phase 02 — seed, provisioning and reconciliation

**Status:** draft  
**Depends on:** [Phase 01](01-domain.md)

## Work

1. Add the system `Client` role to `RoleSeeder` with description `Клієнт`, customer scope and a least-privilege access level consistent with existing role conventions.
2. Replace the imported-counterparty service's current `EnsureUserRoleAsync(... User)` behaviour with `EnsureClientRoleAsync`.
3. If the `Client` role is unavailable, return an `Ardalis.Result` error so `OneCCounterpartiesSyncService` counts the counterparty as skipped and logs the reason. Do not silently continue.
4. Implement a one-time, idempotent reconciliation for existing `Sales.Counterparty → IdentityUserId` links:
   - add `Client`;
   - conditionally remove `User` only from those linked imported accounts;
   - preserve `Admin` and `Manager` and all non-imported accounts;
   - make the execution route explicit (migration-safe seeder, dedicated admin/console job, or deployment task) before writing code.
5. Add tests for new, reimported, staff-role and failure paths.

## Design constraints

- `Sales.Infrastructure` keeps calling `IImportedCounterpartyIdentityProvisioningService`; it must not depend on `Identity.Infrastructure` directly.
- Do not change `GetCounterpartiesAsync`, password reset behaviour or SOAP mapping in this phase.
- Do not use raw inserts into `AspNetUserRoles`; use `UserManager`/`RoleManager` to keep Identity invariants.
- Reconciliation must be observable: report examined, assigned, removed, skipped and failed counts without logging credentials.

## Acceptance criteria

- [ ] Application startup/approved seeding creates `Client` when absent.
- [ ] New and existing imported counterparties have `Client` after successful provisioning.
- [ ] Reconciliation affects only users linked from Sales counterparties.
- [ ] Duplicate `Client` assignments are impossible.
- [ ] Staff roles are preserved.

## Verification

- [ ] Integration test with `AppIdentityDbContext` and `SalesDbContext` where available.
- [ ] Targeted sync run against non-production data.
- [ ] Inspect `AspNetRoles` and `AspNetUserRoles` using only non-sensitive identifiers.
