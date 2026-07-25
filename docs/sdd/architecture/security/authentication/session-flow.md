# Authentication

## Current mechanism

KedrStore uses ASP.NET Core Identity with the bearer-token scheme (`IdentityConstants.BearerScheme`). Identity data and refresh-session records are stored through `AppIdentityDbContext` in PostgreSQL. `Host.Api` enables the authentication middleware before authorization.

```text
Client
  | POST /api/auth/session/login { email, password }
  v
AuthSessionController (AllowAnonymous, AuthLogin rate limit)
  v
SessionLoginCommand -> Mediator behaviors -> IdentitySessionService
  v
UserManager + SignInManager -> AppUserClaimsPrincipalFactory
  v
access token in JSON response + refresh token in HttpOnly cookie
```

`AppUserClaimsPrincipalFactory` places the user id, user name, email, security stamp, user claims and roles into the authenticated principal. Roles are therefore available to authorization policies.

## Session endpoints

| Endpoint | Access | Input and result |
| --- | --- | --- |
| `POST /api/auth/session/login` | `AllowAnonymous`, `AuthLogin` rate limit | Email/password. Returns token type, access token and lifetime; writes refresh and CSRF cookies. |
| `POST /api/auth/session/refresh` | `AllowAnonymous`, `AuthRefresh` rate limit | Requires refresh-token cookie and the configured CSRF header matching the CSRF cookie. Rotates the refresh session and returns a new access token. |
| `POST /api/auth/session/logout` | authenticated | Revokes active refresh sessions, updates the security stamp, then clears refresh and CSRF cookies. |
| `GET /api/auth/session/me` | authenticated | Returns current user id, email and roles. |

The refresh token is never returned in the JSON response. Its cookie is `HttpOnly`; the CSRF cookie is readable by the browser so the frontend can send its value in the configured header for refresh requests.

## Token and session rules

- Access-token and refresh-token lifetimes come from `IdentitySessionSecurityOptions` during `AddBearerToken` registration.
- Each refresh token is stored only as a SHA-256 hash in `RefreshSessions`, together with timestamps and limited client context.
- Refresh sessions have absolute and idle expiry.
- A successful refresh rotates the session: the old one is revoked and linked to its replacement.
- Reuse of a revoked or replaced refresh token revokes all active refresh sessions for that user.
- Logout also changes the user security stamp, invalidating refresh tokens tied to the old stamp.

## Identity options currently configured

- A unique email is required.
- Password length is at least 5 characters.
- Digit, lowercase, uppercase and non-alphanumeric characters are not currently required.

These are current runtime settings, not a recommendation for a public-registration feature. Any public registration design must explicitly review password policy, email confirmation, abuse protection and recovery flow before implementation.

## Frontend use

1. Call `login`; retain the returned access token according to the frontend security design.
2. Send it as `Authorization: Bearer <access-token>` to protected API requests.
3. When refreshing, allow the browser to send cookies and add the configured CSRF-header value from the CSRF cookie.
4. On logout, call `logout` while the access token is still valid, then discard the access token client-side.

Do not put the refresh token into local storage, application logs or a frontend request body.
