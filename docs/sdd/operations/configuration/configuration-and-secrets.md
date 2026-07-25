# Configuration and secrets

## Sources

`Host.Api` uses the standard ASP.NET Core configuration stack. In Development, its project has a User Secrets id; use user secrets for local sensitive values. Deployment configuration should supply sensitive values through its approved secret/environment mechanism.

Environment-variable keys use double underscores, for example `ConnectionStrings__Default` for `ConnectionStrings:Default`.

## Important settings

| Setting | Purpose | Secret |
| --- | --- | --- |
| `ConnectionStrings:Default` | PostgreSQL connection for application contexts | yes |
| `ADMIN_DEFAULT_PASSWORD` | Password used only if the bootstrap administrator must be created | yes |
| `OneCSoap:Endpoint`, `Username`, `Password` | OneC SOAP integration | endpoint is environment-specific; credentials are secret |
| `Telegram:BotToken`, `ChatId` | Telegram integration | yes |
| `Cors:AllowedOrigins` / `AllowedOriginsCsv` | Browser origins allowed to call the API | no, but deployment-specific |
| `Identity:SessionCookies` | Cookie names, paths, lifetime, `Secure` and SameSite rules | no |
| `Identity:SessionSecurity` | Access/refresh token lifetime limits | no |

Set a local secret, for example:

~~~powershell
dotnet user-secrets set "ADMIN_DEFAULT_PASSWORD" "<local-only-value>" --project src/Bootstrapper/Host.Api/Host.Api.csproj
~~~

Never put real passwords, tokens or production connection strings into `appsettings*.json`, Markdown, source code or logs. Do not print configuration values while diagnosing a startup problem.

## Cookie and CORS note

Refresh authentication uses cookies with credentials, so CORS is an explicit allow-list; wildcard origins are not used. The current default refresh cookie has `Secure: true` and `SameSite: None`. Test refresh through HTTPS, or make a deliberate local-only override and never carry it to a shared environment.
