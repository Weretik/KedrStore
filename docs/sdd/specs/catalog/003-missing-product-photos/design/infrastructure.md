# Missing product photos — design: Infrastructure

## Persistence and query

Add `ProductPhotoChecks` to `CatalogDbContext` with a shared `ProductId` key,
required URL snapshot and status, nullable completion/HTTP/diagnostic fields,
and a required foreign key to `Products` using physical cascade delete. Add an
index on `(Status, ProductId)` for the unavailable-status page. The product's
existing query filter remains authoritative for active scope; queries must join
through active `Products`/`ProductListProjections` and use no tracking.

The migration creates the table, constraint, and index, then inserts `Unknown`
rows for existing non-soft-deleted products from their current `Photo` value.
Deleted historical products are not backfilled. Rolling back drops only this
table and its feature state.

The list query joins the status table to the existing admin product sources,
filters `Missing`, `InvalidContentType`, and `CheckFailed`, orders by product ID,
counts, and pages in SQL. It does not materialize the full catalog and does not
call the CDN.

## CDN adapter

Application defines an `IProductPhotoProbe` abstraction and a transport-neutral
result. Infrastructure implements it with a typed `HttpClient`:

- only absolute HTTPS URLs on the configured allow-listed CDN host are sent;
- redirects are bounded and accepted only when each target remains HTTPS and on
  the allow-listed host;
- the initial request is `HEAD`;
- only `405` and `501` trigger `GET` with
  `HttpCompletionOption.ResponseHeadersRead`;
- the fallback response is disposed after status and headers are classified;
- normalized `Content-Type` beginning with `image/` is required for success;
- response bodies and unbounded external messages are never persisted or logged;
- the per-request timeout, batch size, and concurrency have validated,
  configurable defaults.

The design follows [RFC 9110 HTTP semantics](https://datatracker.ietf.org/doc/html/rfc9110#name-head),
which defines `HEAD` as the `GET` metadata response without response content.

## Job execution and observability

`CheckProductPhotosJob` lives in Catalog Application and pages active products
by ascending ID. It probes with bounded in-process concurrency, classifies every
individual result, and commits one bounded batch at a time. A URL comparison is
part of the write condition. Cancellation stops scheduling new probes and is
propagated normally. Individual remote failures become `CheckFailed`; database,
invalid configuration, or unexpected orchestration failures fail the command.

Structured summary logs contain counts for examined, available, missing,
invalid-content, check-failed, stale-result-discarded, and elapsed duration.
Per-item warning logs use product ID, bounded status/diagnostic code, and host;
they do not contain bodies, secrets, or exception dumps for expected failures.

Host.Jobs registers the use case and maps
`--job=check-product-photos` to one run. The CLI documentation records that no
`--rootId` is needed.

## Cloud Run deployment

Extend `.github/workflows/deploy-cloudrun.yml` with create-or-update logic for
`catalog-product-photo-check`, using the existing `JOBS_IMAGE`, region, and
project. Configure:

- argument `--job=check-product-photos`;
- `--tasks 1`, `--parallelism 1`, `--max-retries 0`;
- a `30m` task timeout;
- only `ConnectionStrings__Default=db-connection:latest` as a secret.

The workflow updates the definition but contains no `gcloud run jobs execute`
for this job and no scheduler resource. This matches the one-shot Cloud Run Job
model in the [Google Cloud documentation](https://cloud.google.com/run/docs/create-jobs).

## Migration and rollout

Database migration must run before the new API, job command, and workflow job
definition become active. The endpoint initially returns an empty page because
backfilled rows are `Unknown`; the first operator-scheduled run populates
terminal states. Deploying the Cloud Run definition does not start that run.

Rollback first disables the external schedule, then rolls back API/job code and
drops the feature table through the migration down path. No Product or CDN data
is removed.
