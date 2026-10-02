# Missing product photos — design: Domain

## Responsibility

Introduce `ProductPhotoCheck` as a Catalog-owned operational aggregate keyed by
the existing strongly typed `ProductId`. It has no independent identity: the
shared key is both its primary key and foreign key to `Product`. This follows
the identifier strategy and expresses the one-to-zero-or-one lifecycle.

The aggregate owns the current URL snapshot, status, last-completed timestamp,
nullable HTTP status, and bounded diagnostic code/message. Its behavior is:

- create or reset to `Unknown` for a current URL;
- record `Available`, `Missing`, `InvalidContentType`, or `CheckFailed` with a
  completion timestamp;
- clear stale completion and diagnostics when the URL changes;
- reject a result for a URL other than the stored current URL;
- keep `Unknown` without a completed timestamp and require one for terminal
  states.

The status enum is closed to the five agreed values. HTTP classification itself
does not belong in Domain because it depends on transport semantics; the
Application job passes a classified result into the aggregate.

## Intermodule interaction

All behavior stays inside Catalog. The 1C product-details import reconciles
`ProductPhotoCheck` after creating or updating a `Product`. Host.Jobs invokes a
Catalog Application job. No domain event, cross-module contract, queue, or
distributed transaction is required.

`ProductPhotoCheck` is independently loaded and updated by the background job,
so it is an aggregate root despite sharing the product lifecycle. PostgreSQL
enforces the required parent relationship and cascade only for physical
deletion.

## Risks

- An import can change a URL while a probe is in flight. Persistence must use
  an atomic compare against the URL snapshot so an old result cannot overwrite
  the reset state for a new URL.
- A Cloud Run execution must remain single-task, but an operator could start
  overlapping executions. Result writes must be idempotent for the same URL;
  the later completed check may replace the earlier one.
- A CDN outage can classify many products as `CheckFailed`. That is intentional
  consumer-visible unavailability, while the distinct status prevents it from
  being confused with a confirmed missing object.
