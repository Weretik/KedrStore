# Authentication

## Current mechanism

KedrStore uses ASP.NET Core Identity with the bearer token scheme (`IdentityConstants.BearerScheme`). Identity data and refresh-session records are stored through `AppIdentityDbContext` in PostgreSQL. `Host.Api` enables authentication middleware before authorization.

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

`AppUserClaimsPrincipalFactory` adds the user ID, name, email, security stamp, user claims, and roles to the authenticated principal. This makes roles available to authorization policies.

## Session endpoints

| Endpoint | Access | Input and result |
| --- | --- | --- |
| `POST /api/auth/session/login` | `AllowAnonymous`, `AuthLogin` rate limit | Email/password. Returns token type, access token, and lifetime; sets refresh and CSRF cookies. |
| `POST /api/auth/session/refresh` | `AllowAnonymous`, `AuthRefresh` rate limit | Requires a refresh-token cookie and configured CSRF header whose value matches the CSRF cookie. Rotates the refresh session and returns a new access token. |
| `POST /api/auth/session/logout` | Authenticated user | Revokes active refresh sessions, updates the security stamp, and clears refresh and CSRF cookies. |
| `GET /api/auth/session/me` | Authenticated user | Returns the current user ID, email, and roles. |

The refresh token is never returned in a JSON response. Its cookie is `HttpOnly`; the CSRF cookie is accessible to the browser so the frontend can pass its value in the configured header during refresh.

## Token and session rules

- Access and refresh token lifetimes come from `IdentitySessionSecurityOptions` when `AddBearerToken` is registered.
- Each refresh token is stored in `RefreshSessions` only as a SHA-256 hash, together with timestamps and limited client context.
- Refresh sessions have both an absolute expiration and an inactivity expiration.
- A successful refresh rotates the session: it revokes the old one and links it to the new one.
- Reuse of a revoked or replaced refresh token revokes all active refresh sessions for the user.
- Logout also changes the user's security stamp, invalidating refresh tokens with the old stamp.

## Current Identity settings

- Email must be unique.
- The password must be at least 5 characters long.
- Digits, lowercase and uppercase letters, and non-alphanumeric characters are not currently required.

These are current runtime settings, not a recommendation for a public registration feature. Before implementing one, explicitly review the password policy, email confirmation, abuse protection, and account recovery.

## Frontend usage

1. Call `login` and store the returned access token according to the frontend security design.
2. Send it as `Authorization: Bearer <access-token>` with protected API requests.
3. During refresh, allow the browser to send cookies and add the configured CSRF header with the CSRF cookie value.
4. During logout, call `logout` while the access token is valid, then remove it on the client.

Do not put the refresh token in local storage, application logs, or the frontend request body.
