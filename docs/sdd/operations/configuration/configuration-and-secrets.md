# Configuration and secrets

## Sources

`Host.Api` uses the standard ASP.NET Core configuration sources. Its project has a User Secrets ID for Development; store local sensitive values in User Secrets. `Host.Jobs` also loads its own User Secrets in Development, separate from the API's. During deployment, provide sensitive values through the approved secrets/environment-variable mechanism.

Use double underscores in environment variable keys: for example, `ConnectionStrings__Default` maps to `ConnectionStrings:Default`.

## Important settings

| Setting | Purpose | Secret |
| --- | --- | --- |
| `ConnectionStrings:Default` | PostgreSQL connection for application contexts | Yes |
| `ADMIN_DEFAULT_PASSWORD` | Password used only to create the initial administrator through the migration job | Yes |
| `OneCSoap:Endpoint`, `Username`, `Password` | 1C SOAP integration | Endpoint is environment-specific; credentials are secret |
| `Telegram:BotToken`, `ChatId` | Telegram integration | Yes |
| `Sales:OneCOrderSync` | Batch size, intervals, attempt limit, Kyiv sending window, and recovery for stale `Sent` state | No |
| `Cors:AllowedOrigins` / `AllowedOriginsCsv` | Allowed browser origins for the API | No, but environment-specific |
| `Identity:SessionCookies` | Cookie names, paths, lifetime, `Secure`, and SameSite | No |
| `Identity:SessionSecurity` | Access/refresh token lifetime limits | No |

Example local secret for `Host.Api`:

~~~powershell
dotnet user-secrets set "ADMIN_DEFAULT_PASSWORD" "<local-only-value>" --project src/Bootstrapper/Host.Api/Host.Api.csproj
~~~

For `Host.Jobs --job=migrate`, provide `ADMIN_DEFAULT_PASSWORD` and `ConnectionStrings:Default` through its own User Secrets or environment variables. API User Secrets are not automatically passed to the Jobs host.

Do not put real passwords, tokens, or production connection strings in `appsettings*.json`, Markdown, code, or logs. Do not print configuration values while diagnosing startup.

## Cookies and CORS

Refresh authentication uses cookies with credentials, so CORS has an explicit allow-list; wildcard origins are not used. The current refresh cookie has `Secure: true` and `SameSite: None`. Test refresh over HTTPS or make an intentional local override that is not carried into a shared environment.
