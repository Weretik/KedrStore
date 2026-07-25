# Phase 05 — manual verification

**Status:** draft  
**Depends on:** Phase 02 deployed to a non-production environment

1. Confirm `Counterparty` exists in `AspNetRoles` with description `Контрагент`.
2. Select a non-production 1C counterparty with valid email and price type; do not record credentials/password data.
3. Run `Host.Jobs --job=counterparties`.
4. Locate the Sales counterparty and linked `IdentityUserId`; verify one `Counterparty` role assignment.
5. Re-run the job and verify no duplicate role or counterparty link.
6. Use a controlled linked staff-role account, where permitted, to prove `Admin`/`Manager` survive.
7. Run the approved existing-user reconciliation and verify its counts and selected `User` transition rule.

Do not manually edit `AspNetUserRoles`, print password/reset-token data, or run the reconciliation in production before the parent specification's compatibility questions are resolved.
