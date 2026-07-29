# Database migration job — integration contract

**Status:** internal process contract; no HTTP API is in scope

## Host.Jobs CLI

```text
dotnet Host.Jobs.dll --job=migrate
```

| Result | Exit code | Contract |
| --- | --- | --- |
| All contexts migrated or already current | `0` | logs successful completion per context |
| Invalid job arguments | non-zero | prints a concise argument error |
| Migration failure | non-zero | logs the failing context and rethrows the sanitized failure path |

The command reads the standard configuration and secret-provided `ConnectionStrings__Default`. It does not accept connection strings as CLI arguments and must not print secrets.

## Deployment contract

The deployment workflow must execute the dedicated Cloud Run migration job and wait for success before `gcloud run deploy api`. A failed migration job blocks the API deployment.
