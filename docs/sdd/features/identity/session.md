# User session

**Static code check:** 2026-10-28. **Module:** Identity.

Browser cookie login (Admin, storefront), mobile token login and refresh, logout, and the current user. Rules and reasons are in the [session flow](../../architecture/security/authentication/session-flow.md).

## Structure and relationships

[CookieLoginEndpoint](../../../../src/Modules/Identity/Identity.Api/Endpoints/Auth/CookieLogin/CookieLoginEndpoint.cs) signs a browser in through [CookieSessionService](../../../../src/Modules/Identity/Identity.Infrastructure/Sessions/CookieSessionService.cs). The mobile endpoints ([MobileLoginEndpoint](../../../../src/Modules/Identity/Identity.Api/Endpoints/Auth/MobileLogin/MobileLoginEndpoint.cs) and the refresh and logout endpoints next to it) use [IdentitySessionService](../../../../src/Modules/Identity/Identity.Infrastructure/Sessions/IdentitySessionService.cs), which issues tokens with [IdentityBearerTokenIssuer](../../../../src/Modules/Identity/Identity.Infrastructure/Sessions/IdentityBearerTokenIssuer.cs) and keeps refresh sessions in [RefreshSessionStore](../../../../src/Modules/Identity/Identity.Infrastructure/Sessions/RefreshSessionStore.cs). [GetSessionMeEndpoint](../../../../src/Modules/Identity/Identity.Api/Endpoints/Auth/GetSessionMe/GetSessionMeEndpoint.cs) serves `me` for both kinds of client. [SessionCookieWriter](../../../../src/Modules/Identity/Identity.Api/Endpoints/Auth/Shared/SessionCookieWriter.cs) owns the CSRF cookie; [CookieCsrfMiddleware](../../../../src/Bootstrapper/Host.Api/Security/Csrf/CookieCsrfMiddleware.cs) checks the CSRF header. The `Kedr` scheme is registered in [IdentityAuthenticationExtensions](../../../../src/Modules/Identity/Identity.Infrastructure/DependencyInjection/IdentityAuthenticationExtensions.cs).

## Code structure

```text
src/
├── Modules/Identity/
│   ├── Identity.Api/Endpoints/Auth/
│   │   ├── AuthEndpoints.cs  # groups: AuthSession, CookieAuth, MobileAuth
│   │   ├── CookieLoginEndpoint.cs, CookieLogoutEndpoint.cs  # browser login/logout
│   │   ├── MobileLoginEndpoint.cs, MobileRefreshEndpoint.cs, MobileLogoutEndpoint.cs  # mobile
│   │   ├── GetSessionMeEndpoint.cs  # me (current user)
│   │   └── SessionCookieWriter.cs  # CSRF and refresh cookies
│   ├── Identity.Application/
│   │   ├── Abstractions/Sessions/  # ICookieSessionService, IIdentitySessionService
│   │   └── Features/Auth/
│   │       ├── Commands/CookieLogin/, CookieLogout/  # browser use cases
│   │       ├── Commands/SessionLogin/, SessionRefresh/, SessionLogout/  # token use cases
│   │       └── Queries/SessionMe/  # me
│   └── Identity.Infrastructure/
│       ├── DependencyInjection/IdentityAuthenticationExtensions.cs  # Kedr policy scheme, cookie, bearer
│       └── Sessions/
│           ├── CookieSessionService.cs  # SignInManager cookie sign-in
│           ├── IdentitySessionService.cs  # token login, refresh, logout, me
│           ├── IdentityBearerTokenIssuer.cs  # access token + random refresh token
│           └── RefreshSessionStore.cs  # refresh-session rows, rotation with xmin check
└── Bootstrapper/Host.Api/Security/Csrf/CookieCsrfMiddleware.cs  # CSRF header for cookie sessions
```

## Structure diagram

```mermaid
flowchart LR
    B["Browser"] --> CA["CookieLoginEndpoint"]
    M["Mobile"] --> MA["MobileLoginEndpoint"]
    B --> CSRF["CookieCsrfMiddleware"]
    CA --> CS["CookieSessionService"] --> SIM["SignInManager cookie"]
    MA --> IS["IdentitySessionService"]
    IS --> TI["IdentityBearerTokenIssuer"]
    IS --> RS["RefreshSessionStore"] --> T["AppRefreshSessions"]
    SIM --> DP["DataProtectionKeys"]
    TI --> DP
```

## Tests

Unit: [IdentityInputValidatorsTests](../../../../tests/UnitTests/Identity/Application/IdentityInputValidatorsTests.cs) and [ProfileAndSessionValidatorsTests](../../../../tests/UnitTests/Identity/Application/ProfileAndSessionValidatorsTests.cs) check sign-in and refresh input — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Identity"`.

PostgreSQL integration tests in [Identity/Sessions](../../../../tests/IntegrationTests/Identity/Sessions/) cover lockout, Data Protection keys, the refresh grace window, parallel refresh and logout scope; [Identity/Api](../../../../tests/IntegrationTests/Identity/Api/) covers cookie login, CSRF, and the mobile flow through the real API. They need Docker or `KEDR_IDENTITY_TEST_POSTGRES_CONNECTION`.

`dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Identity"`

## Related sources

[Contract](../../contracts/identity/session.openapi.yaml) · [Staff accounts](staff-accounts.md) · [Provisioning](../../architecture/security/users/provisioning-and-registration.md).
