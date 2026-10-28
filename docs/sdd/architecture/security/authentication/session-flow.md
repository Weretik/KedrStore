# Authentication

## Approach

KedrStore uses ASP.NET Core Identity. Each client signs in the way that fits it:

| Client | Mechanism | Why |
| --- | --- | --- |
| Admin (React) and storefront (Angular) in a browser | Identity session cookie `__Host-kedr.session` (HttpOnly, Secure, SameSite=Lax, sliding 7 days) plus a CSRF header | No token is readable by JavaScript, so an XSS bug cannot steal a session. The API is on the same site (`api.classkedr.com.ua`, see the [Cloudflare API proxy](../../../operations/deployment/cloudflare-api-proxy.md)), so the cookie is first-party. |
| Mobile (Expo) | Bearer access token (15 minutes) plus a rotating refresh token in the request body | Native apps have no browser cookie jar; the app keeps both tokens in the device secure store. |

The default scheme `Kedr` is a policy scheme ([registration](../../../../../src/Identity/Identity.Infrastructure/DependencyInjection/IdentityAuthenticationExtensions.cs)): a request with `Authorization: Bearer …` is authenticated by the bearer scheme, any other request by the Identity cookie. Unauthenticated calls to a protected endpoint get `401` and calls without the required role get `403`; the API never redirects.

```text
Browser                               Mobile
  | POST /api/auth/cookie/login         | POST /api/auth/mobile/login
  v                                     v
CookieAuthController                  MobileAuthController
  v                                     v
CookieSessionService                  IdentitySessionService
  SignInManager.PasswordSignInAsync     CheckPasswordSignInAsync -> IdentityBearerTokenIssuer
  v                                     v                         -> RefreshSessionStore
Set-Cookie: __Host-kedr.session       { accessToken, refreshToken }
Set-Cookie: kedr.csrf
```

`AppUserClaimsPrincipalFactory` adds the user ID, name, email, security stamp, user claims, and roles to the principal, so roles reach authorization policies in both schemes.

## Endpoints

| Endpoint | Access | Behavior |
| --- | --- | --- |
| `POST /api/auth/cookie/login` | Anonymous, `AuthLogin` rate limit | Email/password. Sets the session and CSRF cookies; returns the current user. |
| `POST /api/auth/cookie/logout` | Cookie session, CSRF header | Signs out and expires both cookies. |
| `POST /api/auth/mobile/login` | Anonymous, `AuthLogin` rate limit | Returns `tokenType`, `accessToken`, `expiresIn`, `refreshToken` in JSON. Sets no cookies. |
| `POST /api/auth/mobile/refresh` | Anonymous, `AuthRefresh` rate limit | Body `{ refreshToken }`. Rotates the refresh session and returns a new pair. |
| `POST /api/auth/mobile/logout` | Bearer | Body `{ refreshToken }`. Revokes that refresh session; without it, all of the user's refresh sessions. |
| `GET /api/auth/session/me` | Cookie or bearer | Current user ID, email, full name, and roles. |
| `POST /api/auth/session/login`, `/refresh`, `/logout` | Deprecated | The earlier flow (access token in JSON, refresh token in the `kedr.rt` cookie). Kept until the frontends stop calling it, then removed. |

## Browser session rules

- `SignInManager` writes the session cookie; its lifetime is `Identity:CookieSession:LifetimeDays` (sliding).
- The cookie carries the security stamp. Every `SecurityStampValidationMinutes` (default 5) the stamp is checked, so a password change, a role change, or blocking ends the session within that interval.
- [CookieCsrfMiddleware](../../../../../src/Bootstrapper/Host.Api/Security/Csrf/CookieCsrfMiddleware.cs) applies to requests that carry the session cookie and no bearer header. A `GET`, `HEAD`, or `OPTIONS` request passes and gets a fresh `kedr.csrf` cookie if the old one is gone. Any other request must send `X-CSRF-Token` equal to the `kedr.csrf` cookie, otherwise `400`. Cookie login is exempt, so a stale cookie cannot block signing in; SameSite=Lax covers it.
- The CSRF cookie uses `Identity:SessionCookies:CsrfCookieDomain` (production: `classkedr.com.ua`), so the frontends on other subdomains can read it.
- Local frontend development over plain HTTP needs `Identity:CookieSession:RequireSecure=false` and a `CookieName` without the `__Host-` prefix; the options validation rejects any other combination.

## Mobile token rules

- The access token is protected by the bearer scheme's Data Protection protector; the refresh token is 32 random bytes (Base64Url). [IdentityBearerTokenIssuer](../../../../../src/Identity/Identity.Infrastructure/Services/Sessions/IdentityBearerTokenIssuer.cs) creates both.
- [RefreshSessionStore](../../../../../src/Identity/Identity.Infrastructure/Services/Sessions/RefreshSessionStore.cs) keeps each refresh token in `AppRefreshSessions` only as a SHA-256 hash, with timestamps and limited client context.
- Refresh sessions have an absolute lifetime (`RefreshAbsoluteLifetimeDays`, 30) and an inactivity limit (`RefreshIdleTimeoutDays`, 14).
- A successful refresh rotates the session: it revokes the old one and links it to the new one. Rotation is guarded by the PostgreSQL `xmin` concurrency token, so parallel refresh requests rotate a session once; the others fail.
- A rotated token presented again within `RefreshReuseGraceSeconds` (default 60) fails quietly. This covers parallel requests and retries.
- A rotated token presented later is treated as theft and revokes all active refresh sessions for the user. A token revoked by logout, blocking, or expiry simply fails.

## Shared rules

- Data Protection keys, which protect cookies and access tokens, are stored in the `DataProtectionKeys` table, so sessions survive deploys and work on every Cloud Run instance.
- Email must be unique. The Identity password minimum is 5 characters because imported customer passwords still use it; staff passwords set through the [staff accounts API](../../../features/identity/staff-accounts.md) need at least 8.
- Lockout: 5 wrong passwords lock the account for 15 minutes. It is on for new users, staff, the seeded administrator (`Identity:AdminUser:LockoutEnabled`) and imported counterparties (`Identity:ImportedCounterparties:LockoutEnabled`). Blocking a staff account is a lockout without an end date.
- The first administrator is created by `IdentitySeeder` from `Identity:AdminUser` and the `ADMIN_DEFAULT_PASSWORD` secret. There is no self-registration; administrators create other staff accounts.

## Frontend usage

Browser apps: send every request with `credentials: 'include'`; add `X-CSRF-Token` from the `kedr.csrf` cookie to every non-GET request; on `401` go to the login page; on `403` show "no access". Mobile: keep both tokens in the secure store; on `401` refresh once and retry, and on a failed refresh go to the login screen. Details are in the [frontend integration guide](../../../contracts/frontend-integration.md).

Never put a token in browser storage, application logs, or URLs.
