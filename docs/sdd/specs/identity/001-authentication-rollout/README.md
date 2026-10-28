# 001 — Authentication and authorization rollout

- **Module:** Identity; Host.Api (schemes, policies, middleware); Sales and Catalog controllers (access only)
- **Status:** Backend implemented for phases A, B and C (2026-10-28). Open: kedr-web switches to the new flows (TS-012), then TS-008 removes the deprecated endpoints
- **Created:** 2026-10-28
- **Source:** auth audit of 2026-10-26 (KedrStore `5d60d55`, kedr-web `e8c62b1`); the API host move is done in [Cloudflare API proxy](../../../operations/deployment/cloudflare-api-proxy.md)
- **Frontend counterpart:** kedr-web: Admin login, "no access" page and Users page; storefront cookie session; Mobile login (TS-012)

## Goal and scope

Every client authenticates in the way that fits it, and every non-public endpoint enforces a named policy:

| Client | Target mechanism |
| --- | --- |
| Admin (React SPA) and storefront (Angular SSR) | Identity cookie session on `api.classkedr.com.ua`, no tokens in JavaScript, CSRF header on unsafe requests |
| Mobile (Expo, managers) | Bearer access token + rotating refresh token sent in the request body, stored in the device secure store |

The existing `/api/auth/session/*` token flow (access token in JSON, refresh token in a cookie) is replaced by these two flows and then removed.

**Out of scope:** external identity providers, OpenIddict, SSO with Brevi, MFA, self-registration, password reset by email (they need decisions not taken here). Permission-based policies (`PermissionNames`) and customer-owns-order checks are a later revision.

## Product decisions

- **D-1 — imported customer passwords (decided 2026-10-28).** Customers do not use the storefront cabinet yet. The imported password rule (`CounterpartyId`, reset on every sync) stays unchanged; replacing it is a later revision. `Password.RequiredLength` stays 5 for the same reason.
- **D-2 — staff accounts (decided 2026-10-28).** The first administrator is created by `IdentitySeeder` from configuration (`Identity:AdminUser`, secret `ADMIN_DEFAULT_PASSWORD`); there is no open admin registration. An administrator creates other administrators and managers on an Admin "Users" page.

## Product rules

- **R-001.** Data Protection keys survive restarts and are shared by all API instances; a deploy does not invalidate sessions or tokens.
- **R-002.** Every account is locked for 15 minutes after 5 consecutive failed passwords, including imported customers and the administrator.
- **R-003.** Two refresh requests with the same token within 60 seconds of its rotation (parallel tabs, network retry) get `401` for the late one and do not revoke the user's other sessions. Reuse after that window still revokes all of the user's sessions.
- **R-004.** Only one refresh request can rotate a given refresh session; a concurrent second one fails without creating a second successor.
- **R-005.** Logout ends the current session only. Other devices stay signed in.
- **R-006.** A browser signs in with email and password and receives only an `HttpOnly`, `Secure`, `SameSite=Lax` session cookie; no token reaches JavaScript. Protected requests with that cookie work until it expires or the security stamp changes.
- **R-007.** A request authenticated by the session cookie with method other than `GET`, `HEAD`, or `OPTIONS` must carry `X-CSRF-Token` equal to the `kedr.csrf` cookie; otherwise `400`.
- **R-008.** Mobile signs in with email and password and receives an access token and a refresh token in JSON. Refresh takes the refresh token in the body and rotates it under R-003 and R-004.
- **R-009.** Unauthenticated calls to a protected endpoint return `401` (never a redirect); authenticated calls without the required role return `403`.
- **R-011.** Only an `Admin` can list staff accounts, create an `Admin` or `Manager` account (email, full name, initial password), change a staff account's role, block or unblock it, and set a new password. An administrator cannot block or demote their own account, so the system always keeps an administrator.
- **R-010.** Admin endpoints require the target policy from the [endpoint access table](../../../architecture/security/authorization/roles-and-policies.md#endpoint-access); public catalog, quick order, and login/refresh stay anonymous and rate-limited.

## Acceptance scenarios

| ID | Phase | Given / When / Then |
| --- | --- | --- |
| **SC-001** | A | Given a signed-in user; when the API is redeployed or the request reaches another instance; then the user's token and refresh still work. |
| **SC-002** | A | Given an account; when 5 wrong passwords are sent; then the 6th attempt with the right password returns `401` until 15 minutes pass. |
| **SC-003** | A | Given a refresh token was rotated 10 seconds ago; when the old token is sent again; then the response is `401` and the new session still refreshes. |
| **SC-004** | A | Given a refresh token rotated more than 60 seconds ago; when it is sent again; then all of the user's refresh sessions are revoked. |
| **SC-005** | A | Given two parallel refresh requests with one token; when both run; then exactly one succeeds and one successor session exists. |
| **SC-006** | A | Given a user signed in on two devices; when one logs out; then the other device still refreshes. |
| **SC-007** | B | Given valid credentials; when the browser calls cookie login; then it receives the session cookie and the CSRF cookie, and `GET /me` returns the user. |
| **SC-008** | B | Given a cookie session; when a `POST` without `X-CSRF-Token` is sent; then `400`; with the matching header, the request proceeds. |
| **SC-009** | B | Given a cookie session; when the user's password or roles change; then the session stops working within the security-stamp interval. |
| **SC-010** | B | Given the mobile app; when it logs in and later refreshes with the body token; then it gets a new token pair and the old refresh token is rotated. |
| **SC-011** | C | Given no credentials; when an Admin endpoint is called; then `401`. Given a `User` role; then `403`. Given `Manager` or `Admin`; then the documented success response. |
| **SC-013** | B | Given an administrator; when they create a manager with email, name and password; then the manager signs in to Admin and Mobile with it. A duplicate email returns `409`. |
| **SC-014** | B | Given an administrator; when they block a manager; then the manager cannot sign in and existing sessions stop within the security-stamp interval. Blocking or demoting their own account returns `400`. |
| **SC-012** | C | Given no credentials; when public catalog, quick order, or login is called; then it works as today. |

## Design

**Phase A — hardening, no client change.**
- `AppIdentityDbContext` implements `IDataProtectionKeyContext` (`DataProtectionKeys` table, migration in Identity). `AddDataProtection().PersistKeysToDbContext<AppIdentityDbContext>().SetApplicationName("KedrStore")` in Identity infrastructure registration. The committed `data-protection-keys/` folder is deleted; it is not used.
- `IdentityOptions.Lockout`: `MaxFailedAccessAttempts = 5`, `DefaultLockoutTimeSpan = 15 min`, `AllowedForNewUsers = true`. `ImportedCounterpartyIdentityOptions.LockoutEnabled` and `AdminUserOptions.LockoutEnabled` default to `true`; `IdentitySeeder` aligns an existing administrator with the configured value. Imported users are aligned on their next sync.
- `AppRefreshSession` gets a PostgreSQL `xmin` concurrency token. `RefreshAsync`: a session revoked with reason `Rotated` less than `RefreshReuseGraceSeconds` (default 60) ago returns failure without reuse handling; a `DbUpdateConcurrencyException` on rotation returns failure.
- Logout reads the refresh cookie, revokes that session only (falls back to all sessions when the cookie is missing), updates nothing else. The security stamp is no longer changed on logout, because it ends all cookie sessions in phase B.

**Phase B — per-client sessions.**
- Host.Api authentication: a policy scheme `Kedr` is the default; it forwards to `IdentityConstants.BearerScheme` when the `Authorization` header starts with `Bearer `, otherwise to `IdentityConstants.ApplicationScheme` (Identity cookie). Cookie options: name `__Host-kedr.session`, `HttpOnly`, `Secure`, `SameSite=Lax`, sliding expiration (configurable, default 7 days), `OnRedirectToLogin` → `401`, `OnRedirectToAccessDenied` → `403`. Security stamp validation interval 5 minutes.
- `CookieCsrfMiddleware` (Host.Api) enforces R-007 only when the request was authenticated by the cookie scheme. The CSRF cookie is the existing `kedr.csrf` with `CsrfCookieDomain`.
- Endpoints: `POST /api/auth/cookie/login`, `POST /api/auth/cookie/logout` (Identity, rate-limited); `POST /api/auth/mobile/login`, `POST /api/auth/mobile/refresh`, `POST /api/auth/mobile/logout` (body token, reuse the refresh-session service); `GET /api/auth/session/me` works for both schemes. Contracts in `docs/sdd/contracts/identity/`.
- Refresh tokens become 32 random bytes (Base64Url) stored as SHA-256 hash; access tokens are issued directly with `BearerTokenOptions.BearerTokenProtector` instead of capturing the `SignInAsync` response body.
- The old `/api/auth/session/login|refresh|logout` endpoints are removed after kedr-web stops calling them.

- Staff accounts: `Identity.Application/Features/Users/*` (list, create, change role, block, set password) over a `IStaffAccountService` implemented with `UserManager` in Identity.Infrastructure. `AdminUsersController` at `/api/admin/users` with policy `Identity.Users.Manage` (role `Admin`). Contract in `docs/sdd/contracts/identity/`. Blocking sets `LockoutEnd` to the far future and updates the security stamp; it revokes the user's refresh sessions.

**Phase C — enforcement** (after kedr-web uses the new flows).
- Remove `[AllowAnonymous]` per the anonymous-access table and apply policies; add `401`/`403` to the affected contracts; delete the "current state" section.
- The imported password rule stays (D-1).

## Risks

- Phase A changes lockout for imported customers: a customer who repeatedly mistypes is locked for 15 minutes. Intended.
- The `DataProtectionKeys` migration must run (migration job) before the API that reads it; the deploy workflow already runs the job first.
- Phase B cookie login from `localhost` against the deployed API does not work (cross-site). Local frontend development uses a local API.

## Planned file map

| Path | Phase | State | Responsibility |
| --- | --- | --- | --- |
| `src/Identity/Identity.Infrastructure/DataBase/AppIdentityDbContext.cs` | A | Change | Adds `DataProtectionKeys` set |
| `src/Identity/Identity.Infrastructure/DependencyInjection/IdentityDbContextExtensions.cs` | A, B | Change | Data Protection, lockout options; schemes move to Host.Api in B |
| `src/Identity/Identity.Infrastructure/Migrations/20261028140559_PersistDataProtectionKeysAndRefreshConcurrency.cs` | A | Done | Keys table; `xmin` is a system column, so no DDL |
| `src/Identity/Identity.Infrastructure/Configuration/AppRefreshSessionConfiguration.cs` | A | Change | `xmin` concurrency token |
| `src/Identity/Identity.Infrastructure/Options/IdentitySessionSecurityOptions.cs` | A | Change | `RefreshReuseGraceSeconds` |
| `src/Identity/Identity.Infrastructure/Services/IdentitySessionService.cs` | A, B | Split in A | Use cases. A extracted `RefreshSessionStore.cs` (create, rotate with `xmin`, revoke) and `IdentityBearerTokenIssuer.cs` (token issue). B replaces the response capture with the bearer protector |
| `src/Identity/Identity.Infrastructure/Options/ImportedCounterpartyIdentityOptions.cs`, `AdminUserOptions.cs`, `Seeders/IdentitySeeder.cs` | A | Change | Lockout defaults and alignment |
| `src/Identity/Identity.Api/Identity.Api/Controllers/AuthSessionController.cs` | A, B | Change → split | A: logout of current session. B: `CookieAuthController`, `MobileAuthController`; cookie helpers to `SessionCookieWriter` |
| `src/Identity/Identity.Infrastructure/DependencyInjection/IdentityAuthenticationExtensions.cs` | B | Done | Policy scheme `Kedr`, Identity cookies, bearer; kept in Identity next to the Identity registration instead of Host.Api |
| `src/Identity/Identity.Infrastructure/Services/Sessions/CookieSessionService.cs` | B | Done | Browser sign-in |
| `src/Identity/Identity.Api/Identity.Api/Cookies/SessionCookieWriter.cs` | B | Done | CSRF and refresh cookies, shared by controllers and middleware |
| `src/Identity/Identity.Api/Identity.Api/Controllers/CookieAuthController.cs`, `MobileAuthController.cs`, `AdminUsersController.cs` | B | Done | Browser, mobile, staff accounts |
| `src/Identity/Identity.Application/Features/Users/*`, `Services/IStaffAccountService.cs`, `Infrastructure/Services/Users/StaffAccountService.cs` | B | Done | Staff accounts |
| `src/Bootstrapper/Host.Api/Security/Csrf/CookieCsrfMiddleware.cs` | B | Done | R-007 |
| `src/Sales/Sales.Api/Controllers/Admin*.cs`, `src/Catalog/Catalog.Api/Controllers/Admin*.cs` | C | Done | Policies instead of `[AllowAnonymous]`; Catalog.Api now references Identity.Application for `PolicyNames`, as Sales.Api does |
| `tests/IntegrationTests/Identity/{Sessions,Api,Seeding}/*` | A, B | Done | Refresh grace/race, logout scope, cookie CSRF, mobile flow, staff accounts, seeding |
| `tests/IntegrationTests/TestSupport/Api/TestRoleAuthentication.cs` | C | Done | Shared role header auth for Sales/Catalog API tests (replaced four copies) |
| `docs/sdd/features/identity/session.md`, `architecture/security/authentication/session-flow.md`, `authorization/roles-and-policies.md` | A–C | Change | Current behavior per phase |

## Tasks and traceability

| Task | Phase | Scenarios | Depends on | Test level |
| --- | --- | --- | --- | --- |
| **TS-001** Persist Data Protection keys | A | SC-001 | — | Integration (PostgreSQL) for the keys table; manual after deploy |
| **TS-002** Lockout for all accounts | A | SC-002 | — | Integration (in-memory Identity) |
| **TS-003** Refresh grace window and rotation concurrency | A | SC-003, SC-004, SC-005 | — | Integration; PostgreSQL for `xmin` |
| **TS-004** Logout of the current session only | A | SC-006 | TS-003 | Integration |
| **TS-005** Split session service; direct token issue; random refresh tokens | B | SC-007, SC-010 | TS-003 | Unit + integration |
| **TS-006** Cookie scheme, policy scheme, CSRF middleware, cookie endpoints | B | SC-007, SC-008, SC-009 | TS-005 | Integration (WebApplicationFactory) |
| **TS-007** Mobile endpoints and contract | B | SC-010 | TS-005 | Integration + contract |
| **TS-008** Remove old `/api/auth/session` token endpoints | B | — | kedr-web switched | Contract |
| **TS-009** Apply policies, contracts 401/403 | C | SC-011, SC-012 | TS-006, TS-007, TS-011 | Integration per endpoint |
| **TS-010** Replace imported passwords | — | — | D-1: deferred | — |
| **TS-011** Staff accounts API | B | SC-013, SC-014 | TS-006 | Integration + contract |
| **TS-012** kedr-web: Admin login, no-access page, Users page; storefront cookie session; Mobile login | B | SC-007, SC-010, SC-013 | TS-006, TS-007, TS-011 | kedr-web tests |

## Verification evidence

Phase A, 2026-10-28:

- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Identity|FullyQualifiedName~TrustedProxy"` against local PostgreSQL 16 (`KEDR_IDENTITY_TEST_POSTGRES_CONNECTION`): 13 passed. Covers SC-001 (keys stored in the database), SC-002, SC-003, SC-004, SC-005 (4 parallel requests, run 3 times), SC-006, and the migration script (keys table, no `xmin` DDL).
- Deviation: a refresh token revoked by logout or expiry now fails without reuse handling; only a rotated token counts as reuse. Without this, a stale tab refreshing after logout signed out every device, which broke R-005.
- Deviation: the refresh cookie path widened from `/api/auth/session/refresh` to `/api/auth/session`, so logout receives it. Login and refresh delete the cookie on the old path.

Phases B and C (backend), 2026-10-28:

- `dotnet test tests/IntegrationTests/IntegrationTests.csproj` against local PostgreSQL 16 for Identity: 135 passed, 23 skipped (Sales/Catalog PostgreSQL tests without Docker; unaffected code). `dotnet test tests/UnitTests/UnitTests.csproj`: 106 passed.
- Covered: SC-007 and SC-008 (cookie login, HttpOnly/SameSite cookie, CSRF 400 and pass), SC-010 (mobile login, refresh rotation, logout), SC-011 (anonymous 401, `User` 403, Manager 200 on admin routes), SC-012 (public catalog contract unchanged), SC-013 and SC-014 (create, duplicate 409, block, self-block 400, role and password change). SC-009 relies on Identity's security-stamp validator; not timed in tests.
- Deviation: the refresh token is now random (not a Data Protection ticket); existing refresh sessions keep working because lookup is by hash and the user comes from the session row.
- Found and fixed: the migration job never bound `Identity:AdminUser`, so `IdentitySeeder` could not create the first administrator in production. The binding moved into Identity infrastructure; `IdentitySeederTests` covers it. The administrator login is `admin@classkedr.com.ua`; the password is the `identity-admin-password` secret.
- Deviation: Host.Jobs imported counterparties now have lockout on (R-002); it was off in the jobs configuration.
- Rollout: the backend and kedr-web must deploy together, because the admin API now requires sign-in. Mobile builds made before TS-012 stop working against the API until updated.

