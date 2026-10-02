# Missing product photos — overview and boundaries

## Goal

As an administration client, I want a paged list of active products whose
photo is unavailable so that missing or broken catalog media can be corrected
without making one CDN request per product from the browser.

## In scope

- A one-shot photo-check operation that examines every active Catalog product,
  including products where `ExportToSite` is `false`.
- Persisted availability state for the current product photo URL.
- A paged anonymous read endpoint containing the existing admin product row
  plus the latest photo-check result.
- Treating a missing file, a non-image response, and a failed check as
  unavailable to the API consumer while retaining their distinct diagnostic
  statuses.
- Resetting the check to unknown when the stored photo URL changes.
- Excluding soft-deleted products and removing a dependent check row when its
  product is physically deleted.
- A `Host.Jobs` command and create-or-update deployment of a Cloud Run Job from
  the existing jobs image.
- Structured diagnostics, migration, contract documentation, tests, and an
  operational runbook update.

## Out of scope

- Uploading, changing, or deleting CDN files.
- Hiding products from the public catalog based on photo availability.
- Frontend implementation or browser-side URL checks.
- Authentication or authorization of the new endpoint; it is intentionally
  anonymous for this feature.
- Filtering products by `ExportToSite`.
- An internal scheduler or a Cloud Scheduler resource. The operator owns the
  execution schedule.
- Automatic execution of the photo-check job during deployment.
- A manual HTTP endpoint that starts a check or rechecks one product.

## Actors and external systems

- Administration client: reads the latest persisted unavailable-photo list.
- Catalog import: creates unknown state for a new product and invalidates stale
  state when the photo URL changes.
- Host.Jobs: runs one complete check cycle and exits.
- CDN: answers HTTP probes for product image URLs.
- PostgreSQL: stores product and latest check state.
- GitHub Actions and Google Cloud Run Jobs: create or update the runnable job;
  they do not schedule or execute it.

## Open questions

None. Scheduling, anonymous access, all-active-product scope, HTTP fallback,
and consumer-visible unavailable semantics are agreed.
