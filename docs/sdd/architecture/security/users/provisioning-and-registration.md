# Users, bootstrap and registration

## What exists now

There is no public self-registration endpoint in the current API. The session API only supports login, refresh, logout and `me`.

Current user-creation paths are:

| Path | Behaviour |
| --- | --- |
| Startup bootstrap | `RoleSeeder` creates the seeded roles. `IdentitySeeder` creates the configured administrator only when it does not yet exist and `ADMIN_DEFAULT_PASSWORD` is supplied. |
| Counterparty import | `ImportedCounterpartyIdentityProvisioningService` finds or creates an `AppUser`, synchronizes its fields/password and assigns the `User` role when it exists. |

The default administrator is configured via `AdminUserOptions`; its password is read from `ADMIN_DEFAULT_PASSWORD`. Never commit this value to configuration or documentation.

## Designing a new registration or user-management feature

Treat this as an Identity-module feature. It is not an implementation detail of Catalog, Sales or an API controller.

```text
Identity.Api endpoint
  -> Identity.Application command + validator
  -> application service contract
  -> Identity.Infrastructure implementation
  -> UserManager<AppUser> / RoleManager<AppRole> / AppIdentityDbContext
```

Minimum decisions to put into the feature specification:

- Who may create the user: anonymous self-registration, Admin-only, import-only, or another policy.
- Identity fields and validation: email uniqueness, normalized input, full name and password rules.
- Initial role and whether a caller may choose it. A public request must never be able to grant Admin or Manager.
- Account activation: immediate login, email confirmation, invitation or administrator approval.
- Password setup/reset and recovery design.
- Abuse controls: rate limit, audit logs and any CAPTCHA/email-verification requirement.
- What happens to active sessions when roles, password or account state change.
- API response shape: never return password, refresh token, password-reset token or internal Identity errors.

## Implementation boundaries

- Put the HTTP contract and controller in `Identity.Api`.
- Put command/query, FluentValidation validator, `Ardalis.Result` contract and application-facing interface in `Identity.Application`.
- Put ASP.NET Core Identity calls, `AppUser` persistence, role assignment and token-provider calls in `Identity.Infrastructure`.
- Keep only durable role constants in `Identity.Domain`; do not put HTTP DTOs, `UserManager` or EF concerns there.
- Register any new service through the existing Identity module registration flow and let `Host.Api` compose it through `AddHostServices`.

Use `UserManager` result errors to create a safe, consistent application result. Do not expose raw Identity diagnostics to anonymous callers.
