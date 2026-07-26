# Phase 05 — manual verification

**Status:** draft  
**Depends on:** Phase 02 implemented and deployed to a non-production environment

## Preconditions

- Identity seeding has run and `Client` exists.
- A valid non-production 1C counterparty has a unique email and valid price type.
- Record the counterparty ID without recording credentials or password material.

## Steps

1. Run `Host.Jobs` with `--job=counterparties` in the intended non-production environment.
2. Confirm the final log reports the expected import/update/skip counts.
3. Locate the Sales counterparty by its 1C ID and obtain its linked `IdentityUserId`.
4. In Identity data, verify that user has `Client` exactly once.
5. Verify staff-role preservation using a controlled linked test account that also has `Admin` or `Manager`, if policy permits.
6. Run the same import again; confirm no duplicate role/link and no unexpected password or role errors.
7. Run the approved reconciliation path for an existing linked account that previously had `User`; verify the selected transition rule and preserve evidence of counts.

## Expected result

The imported account has `Client`; existing staff roles remain; repeated synchronization is idempotent; unlinked users and generic Identity accounts are untouched.

## Do not test by

- printing passwords, reset tokens or 1C credentials;
- applying the reconciliation to production before the compatibility question in the parent specification is resolved;
- deleting or editing `AspNetUserRoles` manually.
