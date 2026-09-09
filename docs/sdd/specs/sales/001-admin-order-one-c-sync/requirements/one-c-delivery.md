# Admin order and 1C synchronization — 1C delivery

## Behavior

- `Host.Jobs` runs `SyncOneCOrdersJob` through `--job=sync-one-c-orders`; each run selects a small batch of synchronization records due for delivery (`NextAttemptAtUtc <= now`).
- For each record, Infrastructure maps the persisted order to the existing `CreateSiteRequest` SOAP operation and calls 1C outside the creating HTTP request.
- The request contains the selected counterparty's 1C ID, stable local `OrderId`, order date, agreed comment and item rows. Each row maps to 1C `ProductId`, `Quantity`, and `Amount`.
- The same job checks the synchronous `CreateSiteRequest` response after each send: a confirmed created-document `DocId` finishes delivery and is stored as `OneCDocumentNumber`; a response such as `Не создан...` is a business failure even if `DocId` is non-empty. A transient transport failure schedules a retry, and a business failure is retained for diagnosis without aggressive retries.
- OneC ignores a repeated request with an existing stable `OrderNumber`. A `Sent` record whose attempt has had no persisted outcome for 15 minutes is therefore safe to claim again with the same `OrderNumber`; a newer `Sent` record remains protected from concurrent delivery.

## Rules and invariants

- The delivery state values are `Pending`, `Sent`, `Accepted`, `BusinessError`, `TransportError`, `RetryScheduled`, and `DeadLetter`.
- Retry delays are 5 minutes, 15 minutes, 30 minutes, 60 minutes, then every 3 hours. The configured initial maximum is 10 attempts.
- The initial configuration processes a batch of 20 and delays 5 seconds between SOAP calls; all operational values are configuration-backed.
- Delivery runs only in the explicit Europe/Kyiv sending window. Outside it, the job moves `NextAttemptAtUtc` to the next allowed time. The initial default window is 07:00–20:00, subject to deployment configuration.
- The job runs once, processes its batch, and exits. Cloud Scheduler cadence is an operator-managed deployment setting and is outside this feature's code and workflow scope.
- The same stable `OrderId` is sent on every retry, preventing duplicate 1C documents.
- Secrets, authorization headers, raw sensitive request payloads, and unbounded raw responses must not be persisted or logged.
- A manager may manually schedule a new delivery cycle only from `BusinessError` or `DeadLetter`. The API persists the reason and audit context, reuses the immutable `OrderNumber`, and returns before any SOAP call.
- `Accepted`, `Sent`, `Pending`, `TransportError`, and `RetryScheduled` cannot be manually retried. Concurrent retry requests are resolved through the synchronization concurrency token.

## Acceptance scenarios

1. Given a due pending synchronization record inside the allowed window, when 1C accepts it, then the record is `Accepted`, includes acceptance time and the returned 1C document number, and will not be retried.
2. Given a timeout, network failure, or gateway failure, when the job sends an order, then it records a transport failure and schedules the next retry using the configured backoff.
3. Given an empty 1C `DocId`, when the job sends an order, then it records `BusinessError` with a sanitized `Comment` and does not schedule aggressive automatic retries.
4. Given a record outside the allowed window, when the job runs, then it makes no SOAP call and schedules the record for the next allowed Kyiv time.
5. Given repeated transient failures beyond the configured maximum, when the next attempt is evaluated, then the record transitions to `DeadLetter` for manual processing.
6. Given a record moves to `DeadLetter`, when the transition is saved, then the system sends one Telegram alert with the client, order ID, final error, and an Excel file containing the order lines.
7. Given an accepted order is later returned by an admin order read/status API, then its response includes `oneCDocumentNumber`; the value is `null` while delivery has not been accepted.
8. Given a `BusinessError` or `DeadLetter` order and a non-empty reason, when a manager requests a retry, then the previous failure is audited, the record becomes due `RetryScheduled`, and the HTTP request makes no 1C call.
9. Given an order in any other status, when a manager requests a retry, then the API returns a conflict and does not change delivery state.
