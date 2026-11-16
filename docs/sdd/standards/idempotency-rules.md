# Idempotency rules

An operation is **idempotent** when doing it twice with the same input leaves the same result as doing it once: no second row, no second payment, no second stock movement, no second message. Repeats are normal, not rare: a person presses twice, a phone loses the answer and retries, a proxy times out after the server committed, and every background mechanism here (outbox, scheduler, webhooks, sync jobs) delivers **at least once**. This document says where protection is required, which pattern to use, and how to check it. It extends [database rules](database-rules.md) section 4 (concurrency) and the outbox rule in [module rules](module-rules.md).

## 1. Where protection is required

Decide for every write operation in the SDD design (next to each command or endpoint: what a repeat does and which protection) and record it in the contract description.

| Operation | What a repeat must do | Protection |
| --- | --- | --- |
| Create of money, stock, orders, numbered documents, payroll, anything that leaves the system (Telegram, e-mail, 1C, SMS) | Return the first result; add nothing | **Request key** (section 2) — required |
| Public anonymous form (site order, site request, call-back) | Return the first result; the managers get one message | **Request key** — required; rate limiting is not a substitute |
| Send from an offline or mobile queue | Same as above | **Request key** stored with the queue entry |
| Delta on shared data (add meters to stock, +1 counter, append a ledger line) | Apply once | **Request key** or an operation id on the ledger row with a unique index |
| State transition (send, close, cancel, retry, publish) | Answer from the current state; no second side effect | **State check** (section 3): the transition is a no-op when the target state is already reached; a transition the state forbids answers 409 |
| Replace or set (PUT a value, set a status, set a list) | Same final state | Naturally idempotent: write the target value, never a delta; concurrent edits use a row version ([database rules](database-rules.md) 4) |
| Delete | Nothing is left | Deleting what is already gone answers the same success (or the same 404) every time, never 500 |
| Create of reference data with a natural unique key (code, slug, e-mail, name in a list) | Second copy refused clearly | Unique index + caught violation → 409 with a Ukrainian message; no request key needed |
| Create of a plain record without a natural key (task, note, FAQ item, folder) where a duplicate is only a nuisance | — | The client lock while pending is enough (frontend rules); add a request key when a duplicate costs money or confuses people |
| Reads, searches, previews, calculations without stored effects (also `POST` used for a complex query) | — | Nothing |
| Login, token refresh, logout | — | No request key: refresh rotation keeps a short grace window for a parallel or repeated refresh |

**Rule of thumb for agents:** if repeating the request would create something, move something, or tell someone something a second time, it needs a request key or a state check. If it only overwrites with the same value, it is already idempotent.

## 2. Request key (client-made idempotency key)

- **Who makes it.** The client, once per attempt: when the form opens or on the first submit of a payload. It reuses the same key for every retry of that attempt (timeout, network error, unknown outcome, second press) and makes a new one after a success or when the person changes the data. Frontend details are in the frontend project's rules.
- **Wire format.** New endpoints take the `Idempotency-Key` header with a UUID. A project that already uses a body field (`requestId`) keeps it; record the choice under project specifics. The key is optional on the wire for compatibility, but every own client sends it.
- **Storage.** Saved in the **same `SaveChanges`** as the result, under a **unique index**:
  - a `RequestId` column on the created row with a unique partial index (`HasFilter("\"RequestId\" IS NOT NULL")`) when the result is one row;
  - a separate idempotency record (`Operation`, `Key`, `RequestHash`, result id, `CreatedAtUtc`, `ExpiresAtUtc`) with a unique index on (`Operation`, `Key`) when the result spans rows, the key must expire, or the same key must be checked against the request data;
  - the created row's id itself (the key becomes the id, so the primary key is the guard) when the client may choose the id.
- **Flow in the handler.**
  1. Look the key up. Found and the request data is the same → answer with the saved result (200, same body as the first answer). Found with other data → 409 `idempotency_conflict` with a Ukrainian message.
  2. Otherwise do the work and save the key with the result in one transaction. Side effects go through the outbox, so they are written once with the result.
  3. Catch the unique violation on the key (`UniqueViolation.Is` / `DbUpdateException`): another copy won the race. Clear the tracked changes, read the winner, and answer as in step 1. Never answer 500 to a repeat.
- **Comparing data.** Compare the business fields of the request (or a hash of their canonical JSON), never timestamps or the key itself.
- **Scope and lifetime.** A UUID is unique on its own; scope it by actor or operation when keys come from several clients. Keep keys at least as long as a client may retry (hours for forms, days for offline queues) and remove expired ones in a scheduled task.
- **Actions with a version.** A command that already sends the row version (`Version`, `xmin`) is protected against a blind second write: the repeat gets 409 "stale". When the first call has an external effect (an e-mail, a Telegram message), a request key is still needed so the repeat answers success instead of a confusing conflict.

## 3. Handlers that run at least once

Every outbox handler, webhook handler, scheduled task, sync job, and import must be safe to run twice with the same input.

- **State check first.** The handler reads the target state and returns when the work is done (`NotifiedAt` is set, the delivery is no longer `Sending`, the order is already accepted by 1C). The state change is saved in the same transaction as the outbox row is marked processed.
- **Dedupe key at the receiver.** A handler that writes rows stores the message id, the external event id (Telegram `update_id`, a webhook event id, a bank statement line fingerprint), or the source's natural key under a unique index and skips a known one.
- **Upsert, not insert.** Sync and import jobs match by the source's natural key (1C code, external id, file fingerprint) and update or insert; a rerun of the same file or period changes nothing.
- **External systems without a dedupe key** (Telegram send, e-mail): mark the work done in the same transaction that completes the outbox row. A crash between the send and the commit can still produce one duplicate; accept it and write it down, but never resend once marked. When the external system accepts a key or a number (1C document number, a message reference), send it so the other side can drop the repeat.
- **Webhooks** answer 2xx for an event they have already handled, so the sender stops retrying; they verify the secret first.
- **Scheduled tasks** run one at a time (advisory lock or scheduler group, [database rules](database-rules.md) 4) and a rerun for the same period skips the work already done (a period marker or a per-item state).
- **Long jobs** are split into steps; each step is one transaction and can be rerun ([database rules](database-rules.md) 3.3).

## 4. Tests

Write them first (Red → Green), next to the use case's other tests:

- Unit: a repeat with the same key returns the first result and saves and sends nothing; the same key with other data answers 409; without a key two requests are two results; a copy that loses the race answers with the winner.
- Integration on real PostgreSQL: two parallel requests with one key → one row and both 2xx; a handler run twice with one message → one side effect.
- Contract: the header or field and the 409 answer are in the OpenAPI document.

## 5. Check (audits)

- Every write endpoint from section 1 that needs a request key has one, stored under a unique index in the same transaction as the result.
- The unique violation on the key is caught and answered with the saved result, never 500.
- A state transition with an external effect is a no-op when repeated.
- Every outbox handler, webhook, scheduled task, and import has a state check, a dedupe key, or an upsert; long jobs are rerunnable steps.
- The contract documents the key and the 409; the client sends it (frontend audit).
- Known gaps are listed under «Known deviations» below with the risk and the plan.

## KedrStore specifics

- **Wire format:** the `Idempotency-Key` header (UUID) on every keyed endpoint. Admin and mobile orders and drafts use the idempotency records `sales."OrderIdempotencyRecords"` (unique (`Operation`, `IdempotencyKey`), `RequestHash`, `ExpiresAtUtc`; each order create removes expired ones) and `sales."OrderDraftIdempotencyRecords"` (unique key, `RequestHash`; one row per draft, kept) and answer 409 `idempotency_conflict` for the same key with other data. The site quick order answers that case with the generic 409 problem (`conflict`, Ukrainian detail) and uses the key as the order id and the outbox message id (the primary key is the guard; repeats are recognised for the 30-day outbox retention).
- **Race guard helper:** `UniqueViolation.Is` (`BuildingBlocks.Infrastructure/Persistence`).
- **Outbox:** `ModuleOutbox` locks rows with `FOR UPDATE SKIP LOCKED`, runs the handler and marks the row in one transaction; a handler is called again only when that transaction did not commit.

### Audit 2026-11-15: write operations and handlers

| Operation or handler | What a repeat does | Verdict |
| --- | --- | --- |
| `POST /api/admin/orders`, `POST /api/admin/order-workspace/orders`, `.../drafts` (admin and mobile «Кедр Менеджер») | Same key → the first order or draft; other data → 409 | Protected (idempotency records). Admin and mobile send the key; the mobile outbox stores it with the queue entry |
| `POST /api/orders` (site quick order) | Same key → the first order id, no second Telegram file | Protected since this change; the Next.js site sends the key |
| Outbox: `SendOrderToOneCHandler` (order to 1C) | Returns at once when the order already has the 1C answer; a transport retry resends the same `OrderNumber`, which 1C treats as the same document | Protected (state check, duplicate-safe number, outbox row locked with `FOR UPDATE SKIP LOCKED`) |
| Outbox: `OrderOneCDeliveryFailedHandler` (Telegram alert) | Queued once in the same transaction that gives up; sent once per row | Accepted risk, as for the quick order (Telegram has no dedupe key) |
| Outbox: `RemovedCounterpartyIdentityDeleter` | A user already gone counts as deleted; a user linked again is kept | Protected (state check) |
| `POST /api/admin/orders/{orderId}/sync/retry` | Puts the same outbox row back in the queue; a repeat on a waiting order is 409 | Protected (state check, conditional update of the row) |
| Outbox: `QuickOrderTelegramSender` | Sent once per row; a crash between the send and the commit can send one duplicate file | Accepted risk (Telegram has no dedupe key) |
| Scheduler tick, «Запустити зараз» | Claims tasks with a `Version` token; «run now» sets a flag | Protected (row version, catalog group, advisory lock in the rebuild) |
| 1C catalog, price, and counterparty syncs | Upsert by 1C codes in steps | Protected (upsert, rerunnable steps) |
| Admin creates with a natural key: staff account (e-mail), catalog attribute and value (code), product group and site model (slug), article (slug), gallery item, media file | Second copy refused by a unique index | Protected (unique index) |
| Admin creates without a natural key: work tasks, home FAQ items and brands, media folders, legal document drafts | A double press could add a second item | Acceptable: the admin buttons are disabled while the request runs and nothing leaves the system; add a key if a duplicate becomes harmful |
| Updates (`PUT` of names, slugs, texts, settings, order of items), deletes | Same final state | Naturally idempotent |
| Uploads (avatar, swatch, article cover, media file, gallery photos) | A repeat stores another file object | Acceptable: the admin is locked while uploading; orphan files are a storage cost, not a business duplicate |
| Mobile refresh token | A parallel or repeated refresh within the rotation grace window is refused without revoking the family | By design (security); no key |

## Known deviations

None open. Record a new gap here with the risk and the plan, and remove the row when it is fixed.
