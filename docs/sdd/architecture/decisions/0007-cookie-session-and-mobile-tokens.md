# ADR-0007: Cookie session for browsers, bearer and refresh tokens for the mobile app

- Status: accepted
- Date: 2026-11-15 (recorded from the current code; cookie session since 2026-10-28)
- Owner: Виталий (project owner)

## Context

Three kinds of clients call one API: the admin and the site in a browser, and the manager app «Кедр Менеджер» on Android. A token kept in browser storage can be stolen by any XSS bug. A native app has no browser cookie jar and must work offline for days.

## Alternatives considered

- JWT in `localStorage` for every client: rejected; readable by JavaScript.
- Cookies for the mobile app too: rejected; React Native has no reliable cookie jar and no CSRF need.
- An external identity provider: rejected; staff accounts are few and managed in the admin.

## Decision

- ASP.NET Core Identity owns accounts, passwords, roles and lockout (Identity module).
- Browsers get the HttpOnly `__Host-kedr.session` cookie (Secure, SameSite=Lax, sliding 7 days) and a double-submit CSRF cookie plus header.
- The mobile app gets a 15-minute bearer access token and a rotating refresh token, kept in the device secure store.
- The default scheme `Kedr` is a policy scheme: a request with `Authorization: Bearer` goes to the bearer scheme, any other to the cookie. The API answers 401/403 and never redirects.
- Endpoints are protected by named policies from `Identity.Contracts` (`PolicyNames`, `RoleNames`); a module never calls `UserManager` directly.

## Why

- No token is readable by browser JavaScript.
- The mobile app keeps working with short-lived tokens and silent refresh.
- One user store and one set of policies for every client.

## Trade-offs

- Two sign-in flows to test and keep in step.
- The cookie must be first-party, which needs the API on the same site ([ADR-0003](0003-cloudflare-worker-api-front-door.md)).

## Sources and checks

- [Session flow](../security/authentication/session-flow.md), [roles and policies](../security/authorization/roles-and-policies.md).
- Integration tests under `tests/IntegrationTests` cover both schemes and the CSRF check.
