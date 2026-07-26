# Phase 02 — seed, provisioning and reconciliation

**Status:** in progress
**Depends on:** [Phase 01](01-domain.md)

## Work

1. Seed the system role `Counterparty` with display description `Контрагент`, customer scope and least-privilege access level consistent with existing roles.
2. Replace `EnsureUserRoleAsync(... User)` in `ImportedCounterpartyIdentityProvisioningService` with idempotent `EnsureCounterpartyRoleAsync`.
3. If the role does not exist, return an `Ardalis.Result` error so `OneCCounterpartiesSyncService` logs and counts the row as skipped.
4. Design and implement a one-time reconciliation for existing `Sales.Counterparty → IdentityUserId` links: add `Counterparty`, conditionally remove `User`, preserve `Admin`/`Manager`, leave unlinked users unchanged, and report counts.
5. Test new, reimported, staff-role and missing-role paths.

## Constraints

- Continue using `IImportedCounterpartyIdentityProvisioningService`; Sales must not reference Identity Infrastructure directly.
- Use `UserManager`/`RoleManager`, never raw writes to `AspNetUserRoles`.
- Do not change SOAP mapping or password/source-field logic in this role-only feature.
- Make the reconciliation execution path explicit before code is written: approved seeder, dedicated job or deployment operation.

## Acceptance criteria

- [x] `Counterparty` is created when seeding runs.
- [x] New and reimported accounts receive it exactly once.
- [ ] Reconciliation touches only Identity users linked from Sales counterparties.
- [ ] Staff roles survive import and reconciliation.

## Implemented in this pass

- `RoleSeeder` now creates `Counterparty` with scope `counterparty`.
- Imported-counterparty provisioning now ensures `Counterparty` idempotently instead of `User`.
- Missing or failed role assignment returns an `Ardalis.Result` error, so the Sales importer skips and logs the counterparty rather than reporting a complete import.
- The existing-user reconciliation and the corresponding integration tests remain pending; no existing `User` assignment was removed automatically.

## Verification result

- `dotnet build src/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj --no-restore` succeeded with 0 errors.
- No dedicated Identity/Sales integration test project is currently present in the repository.
- A pre-existing nullable warning remains in `RoleSeeder` at `FindByNameAsync(role.Name)`; it is unrelated to the Counterparty-role behaviour.
