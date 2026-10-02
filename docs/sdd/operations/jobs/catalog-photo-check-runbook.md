# Catalog product-photo check

`check-product-photos` performs one pass over every active Catalog product and
then exits. It ignores `ExportToSite`, stock, category, sale, and new-product
flags. Host.Jobs contains no scheduler; configure the recurring invocation of
the Cloud Run Job separately.

## Local execution

```powershell
Set-Location C:\Users\Віталій\RiderProjects\KedrStore
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=check-product-photos
```

Supply `ConnectionStrings__Default` through Host.Jobs User Secrets or an
environment variable. The command needs no 1C or Telegram secret and no
`--rootId`.

## Configuration

`ProductPhotoCheck` supports:

| Setting | Default | Constraint |
| --- | ---: | --- |
| `BatchSize` | 500 | 1–5000 |
| `MaxConcurrency` | 16 | 1–100 |
| `RequestTimeoutSeconds` | 5 | 1–120 |
| `MaxRedirects` | 3 | 0–10 |
| `AllowedHosts` | `images-kedr.cdn.express` | at least one non-empty host |

Keep the allow-list limited to the CDN. Redirects outside HTTPS and the allowed
host are rejected without sending a follow-up request.

## Result interpretation

- `Available`: final `2xx` response with `Content-Type: image/*`.
- `Missing`: `404` or `410`.
- `InvalidContentType`: final `2xx` without an image media type.
- `CheckFailed`: timeout, network/DNS problem, invalid URL, `429`, `5xx`, or
  another non-success result.

The anonymous unavailable-photo API includes the final three statuses.
Individual CDN failures do not fail the job. A database, invalid configuration,
cancellation, or unexpected orchestration failure exits non-zero.

The final structured log reports examined, available, missing,
invalid-content, check-failed, stale-result, and elapsed counts. Expected remote
failures never log response bodies or credentials.

## Cloud Run

Deployment creates or updates `catalog-product-photo-check` with one task,
parallelism one, platform retries zero, and a 30-minute timeout. Deployment does
not execute it and does not create Cloud Scheduler. Configure or pause its
schedule in Google Cloud separately.

Before the first successful run, existing backfilled records remain `Unknown`
and do not appear in the unavailable list. If a run is cancelled, already
committed batches remain valid and the next scheduled run checks the full active
catalog again.
