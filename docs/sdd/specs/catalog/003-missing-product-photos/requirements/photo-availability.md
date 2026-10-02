# Missing product photos — photo availability tracking

## Behavior

- Each active product has a latest check state tied to its current photo URL.
- Running the photo-check job examines every active product once per run and
  records a terminal result without stopping the batch for an individual URL
  failure.
- The API lists only products whose latest terminal result is `Missing`,
  `InvalidContentType`, or `CheckFailed`.
- `Available` and `Unknown` products do not appear. `Unknown` means no completed
  check exists for the current URL, so absence has not yet been established.
- A CDN file disappearing changes the next completed result to `Missing`; its
  diagnostic row remains because that row is the evidence used by the API.

## Business rules

### R-001 — Current URL owns current state

A check result applies only to the exact photo URL recorded with it. A new
product or changed URL has `Unknown` state until that URL is checked.

### R-002 — Successful image

A final HTTP response in the `2xx` range with a media type whose normalized
type is `image/*` is `Available`.

### R-003 — Unavailable image classifications

`404` and `410` are `Missing`. A final `2xx` response whose media type is
missing or is not `image/*` is `InvalidContentType`. A timeout, DNS/network
failure, invalid or disallowed URL, `429`, any `5xx`, and every other
non-success result is `CheckFailed`.

All three non-available terminal statuses appear in the API. Their distinct
status and bounded diagnostic data remain available for troubleshooting.

### R-004 — HEAD with bounded GET fallback

The checker first sends `HEAD`. On `405` or `501` only, it retries the same URL
with `GET`, completes after response headers are available, and does not read
the complete image body. This follows the HTTP definition of `HEAD` while
handling an origin that does not implement it.

### R-005 — Complete active catalog scope

Every non-soft-deleted product is eligible, regardless of stock,
`ExportToSite`, category, sale flag, or new-product flag. Soft-deleted products
are neither probed nor returned.

### R-006 — Bounded execution

The job processes deterministic product-ID batches with configurable batch
size, request timeout, and maximum in-process concurrency. Cancellation stops
new work and leaves already committed batches valid. One product failure is
persisted as `CheckFailed` and does not fail the run; a database, configuration,
or unhandled orchestration failure produces a non-zero job exit.

### R-007 — Fast persisted read

The unavailable-photo API reads PostgreSQL only, is paged, orders by product
ID ascending, and performs no CDN calls. Page defaults are `page=1` and
`pageSize=20`; `pageSize` cannot exceed `100`.

### R-008 — Lifecycle integrity

Physical product deletion removes its dependent photo-check row. Soft deletion
preserves the row but removes the product from job and API scope. Restoring a
product makes its saved current-URL state eligible again; a changed URL is
first reset to `Unknown` under R-001.

### R-009 — Anonymous read access

The endpoint is explicitly anonymous during this feature and exposes no raw
exception, response body, credential, connection data, or unbounded diagnostic
message.

### R-010 — Deployment without scheduling

The production workflow creates or updates one Cloud Run Job from the existing
Host.Jobs image with the photo-check command and database secret. Deployment
does not execute the job and does not create its schedule.

## Acceptance scenarios

### SC-001 — Available image is excluded

**Covers:** R-001, R-002, R-004, R-005

Given an active product with its current photo URL
When a check receives a successful image response
Then the latest state is `Available`
And the product is absent from the unavailable-photo list.

### SC-002 — Missing CDN file is listed

**Covers:** R-003, R-005, R-007

Given an active product whose current photo URL returns `404` or `410`
When the job completes and the list is requested
Then the latest state is `Missing`
And the paged API contains the product and check metadata.

### SC-003 — Non-image success is listed

**Covers:** R-002, R-003

Given an active product whose photo URL returns `2xx`
When the response media type is absent or is not `image/*`
Then the latest state is `InvalidContentType`
And the product appears as unavailable.

### SC-004 — Technical check failure is isolated and listed

**Covers:** R-003, R-006

Given multiple active products
When one probe times out or receives another technical failure
Then that product is saved as `CheckFailed`
And checking continues for the remaining products
And the failed product appears as unavailable.

### SC-005 — HEAD fallback avoids full image download

**Covers:** R-004

Given a CDN endpoint that answers `HEAD` with `405` or `501`
When the product is checked
Then the checker issues one fallback `GET`
And classifies the response from its status and headers without consuming the
complete image body.

### SC-006 — Changed URL invalidates old evidence

**Covers:** R-001

Given a completed result for a product photo URL
When import changes that product's photo URL
Then the result becomes `Unknown` for the new URL
And the old result cannot cause the product to appear in the unavailable list.

### SC-007 — Product lifecycle controls dependent state

**Covers:** R-005, R-008

Given a product and its photo-check row
When the product is soft-deleted
Then neither the job nor API includes it
When the product is later physically deleted
Then its photo-check row is removed automatically.

### SC-008 — Paged endpoint is anonymous and database-only

**Covers:** R-007, R-009

Given persisted available, unavailable, and unknown product states
When an anonymous client requests a valid page
Then it receives `200` with only unavailable rows ordered by product ID
And no CDN request occurs.

### SC-009 — Invalid paging is rejected

**Covers:** R-007

Given a page below `1`, a page size below `1`, or a page size above `100`
When the list is requested
Then the client receives the established `400` validation response.

### SC-010 — Cloud Run Job is deployed but not run

**Covers:** R-010

Given a successful production deployment
When the workflow updates Cloud Run resources
Then `catalog-product-photo-check` exists with
`--job=check-product-photos`, one task, parallelism one, platform retries zero,
and the database secret
And the workflow neither executes nor schedules it.

### SC-011 — Job scope ignores export flag

**Covers:** R-005

Given two active products whose `ExportToSite` values differ
When the job runs
Then both products are checked and receive a result.

## Deferred scenarios

- Authorization and role-based access for the admin endpoint.
- Operator-owned Cloud Scheduler configuration.
- Manual single-product recheck and frontend repair workflow.
