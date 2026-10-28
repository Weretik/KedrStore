# Catalog Product Photo Check

`check-product-photos` checks every active Catalog product once and exits. It ignores `ExportToSite`, stock, category, and sale/new-arrival flags. `Host.Jobs` has no scheduler; recurring Cloud Run Job execution is configured separately.

## Local execution

```powershell
# Run from the repository root
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=check-product-photos
```

Provide `ConnectionStrings__Default` through `Host.Jobs` User Secrets or an environment variable. The command does not require 1C or Telegram secrets or the `--rootId` argument.

## Configuration

`ProductPhotoCheck` supports:

| Parameter | Default | Constraint |
| --- | ---: | --- |
| `BatchSize` | 500 | 1–5000 |
| `MaxConcurrency` | 16 | 1–100 |
| `RequestTimeoutSeconds` | 5 | 1–120 |
| `MaxRedirects` | 3 | 0–10 |
| `AllowedHosts` | `images-kedr.cdn.express` | At least one non-empty host |

Limit the allow-list to the required CDN. A redirect outside HTTPS or an allowed host is rejected without making another request.

## Result interpretation

- `Available`: final response `2xx` with `Content-Type: image/*`.
- `Missing`: `404` or `410`.
- `InvalidContentType`: final response `2xx` without an image media type.
- `CheckFailed`: timeout, network/DNS failure, invalid URL, `429`, `5xx`, or another unsuccessful result.

The unavailable-photos API (Manager or Admin) includes the last three statuses. Individual CDN failures do not fail the job. A database failure, invalid configuration, cancellation, or unexpected coordination failure produces a nonzero exit code.

The final structured log contains the examined, available, missing, invalid-content, check-failed, and stale-result counters, plus duration. Expected remote failures do not log response bodies or credentials.

## Cloud Run

The deployment process creates or updates `catalog-product-photo-check` with one task, parallelism 1, zero platform retries, and a 30-minute timeout. Deployment does not execute it or create Cloud Scheduler. The schedule in Google Cloud is configured or stopped separately.

Until the first successful run, existing backfilled records remain `Unknown` and do not appear in the unavailable list. If execution is cancelled, already committed batches remain in effect, and the next scheduled run checks the entire active catalog again.
