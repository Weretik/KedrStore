# User Provisioning and Registration

## Current state

The current API has no public self-registration endpoint. The session API supports only login, refresh, logout, and `me`.

Current user-provisioning paths:

| Path | Behavior |
| --- | --- |
| Separate `Host.Jobs --job=migrate` | `RoleSeeder` creates the defined roles. `IdentitySeeder` creates the configured administrator only if one does not already exist and `ADMIN_DEFAULT_PASSWORD` is provided. A normal `Host.Api` startup does not run seeders. |
| Counterparty import | `ImportedCounterpartyIdentityProvisioningService` finds or creates an `AppUser`, synchronizes its fields/password, and assigns the `User` role if it exists. |

The initial administrator is configured through `AdminUserOptions`; the password is read from `ADMIN_DEFAULT_PASSWORD`. Never commit this value to configuration or documentation. Other administrators and managers are created by an administrator through the [staff accounts](../../../features/identity/staff-accounts.md) API; there is no self-registration.

## Designing new registration or user management

Treat this as an Identity module feature, not an implementation detail of Catalog, Sales, or an API controller.

```text
Identity.Api endpoint
  -> Identity.Application command + validator
  -> application service contract
  -> Identity.Infrastructure implementation
  -> UserManager<AppUser> / RoleManager<AppRole> / AppIdentityDbContext
```

Minimum decisions for the feature specification:

- Who creates the user: anonymous self-registration, Admin only, import only, or another policy.
- Identity fields and validation: email uniqueness, login normalization, full name, and password rules.
- Initial role and whether the caller can choose it. A public request must never grant Admin or Manager.
- Activation: immediate login, email confirmation, invitation, or administrator approval.
- Password setup/reset and account recovery.
- Abuse protection: rate limits, audit logs, and whether CAPTCHA/email confirmation is needed.
- What happens to active sessions after a role, password, or account-status change.
- API response shape: never return a password, refresh token, password-reset token, or internal Identity errors.

## Implementation boundaries

- Put the HTTP contract and controller in `Identity.Api`.
- Put the command or query, FluentValidation validator, `Ardalis.Result` contract, and Application interface in `Identity.Application`.
- Put ASP.NET Core Identity calls, `AppUser` persistence, role assignment, and token-provider calls in `Identity.Infrastructure`.
- Keep only stable role constants in `Identity.Domain`; do not put HTTP DTOs, `UserManager`, or EF dependencies there.
- Register a new service through the existing Identity module path; `Host.Api` composes it through `AddHostServices`.

For `UserManager` result errors, produce a safe and consistent Application result. Do not expose raw Identity diagnostic data to anonymous callers.
