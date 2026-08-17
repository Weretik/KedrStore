# Admin order and 1C synchronization — overview and boundaries

## Goal

As a sales manager, I want to create an order for a selected existing counterparty so that it is retained in Sales and reliably sent to 1C without creating duplicate 1C documents.

## Within

- A new manager/admin order flow in the Sales module.
- Selection of a locally known, active Sales `Counterparty` by its 1C identifier supplied by the frontend.
- Local persistence of the order, its lines, and a pending 1C delivery record in one transaction.
- Asynchronous delivery through the existing SOAP `CreateSiteRequest` operation, retry scheduling, delivery states, observability, and time-window rules from `SALES_1C_SYNC_INSTRUCTION.txt`.
- A protected HTTP endpoint and its versioned OpenAPI contract.

## Out of bounds

- The anonymous Catalog `POST /api/orders` quick-order flow, including its Excel export and Telegram notification.
- Creating or importing counterparties during manager order creation.
- Editing, cancelling, viewing, or manually resending manager orders; these are future use cases unless added by a change note.
- Changes to the generated SOAP proxy or to the 1C WSDL.
- Frontend implementation and Cloud Scheduler/Cloud Run deployment configuration. The job must be schedulable by the existing host, but infrastructure-as-code is not part of this feature.

## Agreed baseline decisions

- `Item.Amount` receives the total amount for the complete order line. For a quantity of 10, `Amount` is the amount for all 10 units, not the price of one unit.
- The endpoint is `POST /api/admin/orders` and is temporarily anonymous. Manager/Admin authorization is a required follow-up before production use.
- `Idempotency-Key` is required on create requests. The frontend creates an opaque UUID for one button action and reuses it only if that same action is retried. Sales keeps a separate idempotency record with a canonical request hash and original result: the same key/hash returns the original order result; the same key with another hash returns `409 Conflict`.
- A non-empty `RequestDataOut.DocId` means `Accepted`. An empty `DocId` returned without a SOAP or transport failure means `BusinessError`; its `Comment` is stored only in sanitized and bounded form.
- Runtime defaults are: `MaxAttempts = 10`, batch size `20`, inter-request delay `5 seconds`, sending window `07:00–20:00 Europe/Kyiv`, and retry schedule `5m → 15m → 30m → 60m → 3h`. Cloud Scheduler cadence is configured manually by the operator after deployment.
